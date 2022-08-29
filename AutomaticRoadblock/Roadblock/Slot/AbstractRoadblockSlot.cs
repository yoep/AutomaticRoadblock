using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Vehicles;
using LSPD_First_Response.Mod.API;
using Rage;
using VehicleType = AutomaticRoadblocks.Vehicles.VehicleType;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    /// <summary>
    /// Abstract implementation of the <see cref="IRoadblockSlot"/>.
    /// This slot defines the entities & scenery items used within the <see cref="IRoadblock"/>.
    /// <remarks>Make sure that the <see cref="Initialize"/> method is called within the constructor after all properties/fields are set for the slot.</remarks>
    /// </summary>
    public abstract class AbstractRoadblockSlot : IRoadblockSlot
    {
        protected const int VehicleHeadingMaxOffset = 10;
        protected static readonly Random Random = new();

        protected readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        protected readonly IGame Game = IoC.Instance.GetInstance<IGame>();

        private readonly bool _shouldAddLights;

        protected AbstractRoadblockSlot(Road.Lane lane, BarrierType barrierType, VehicleType vehicleType, float heading, bool shouldAddLights,
            bool recordVehicleCollisions)
        {
            Assert.NotNull(lane, "lane cannot be null");
            Assert.NotNull(barrierType, "barrierType cannot be null");
            Assert.NotNull(vehicleType, "vehicleType cannot be null");
            Assert.NotNull(heading, "heading cannot be null");
            Lane = lane;
            BarrierType = barrierType;
            VehicleType = vehicleType;
            Heading = heading;
            RecordVehicleCollisions = recordVehicleCollisions;
            _shouldAddLights = shouldAddLights;

            if (VehicleType != VehicleType.None)
                VehicleModel = VehicleFactory.CreateModel(VehicleType, Position);
        }

        #region Properties

        /// <inheritdoc />
        public Vector3 Position => Lane.Position;

        /// <inheritdoc />
        public float Heading { get; }

        /// <inheritdoc />
        public VehicleType VehicleType { get; }

        /// <summary>
        /// The game instances of this slot.
        /// </summary>
        protected List<InstanceSlot> Instances { get; } = new();

        /// <inheritdoc />
        public Vehicle Vehicle => VehicleInstance?.GameInstance;

        /// <inheritdoc />
        public Model VehicleModel { get; }

        /// <inheritdoc />
        public Road.Lane Lane { get; }

        /// <inheritdoc />
        public IEnumerable<ARPed> Cops => Instances
            .Where(x => x.Type == EntityType.CopPed)
            .Select(x => x.Instance)
            .Select(x => (ARPed)x)
            .ToList();

        /// <summary>
        /// The barrier type that is used within this slot.
        /// </summary>
        public BarrierType BarrierType { get; }

        /// <summary>
        /// The indication if the spawned vehicle should record collisions.
        /// </summary>
        protected bool RecordVehicleCollisions { get; }

        /// <summary>
        /// Get the AR vehicle instance of this slot.
        /// </summary>
        protected ARVehicle VehicleInstance => Instances
            .Where(x => x.Type == EntityType.CopVehicle)
            .Select(x => x.Instance)
            .Select(x => (ARVehicle)x)
            .FirstOrDefault();

        /// <summary>
        /// Get the cop ped instance(s) of this slot.
        /// </summary>
        protected IEnumerable<ARPed> CopInstances
        {
            get
            {
                return Instances
                    .Where(x => x.Type == EntityType.CopPed)
                    .Select(x => x.Instance)
                    .Select(x => (ARPed)x);
            }
        }

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => Instances.Count > 0 && Instances.First().IsPreviewActive;

        /// <inheritdoc />
        public void CreatePreview()
        {
            Logger.Debug($"Creating a total of {Instances.Count} instances for the roadblock slot preview");
            Logger.Trace($"Roadblock slot instances: \n{string.Join("\n", Instances.Select(x => x.ToString()).ToList())}");
            Instances.ForEach(x => DoSafeOperation(x.CreatePreview, $"create instance slot {x} preview"));

            if (Instances.Any(x => x.State == InstanceState.Error))
                Game.DisplayNotification(IoC.Instance.GetInstance<ILocalizer>()[LocalizationKey.RoadblockInstanceCreationFailed]);

            DrawRoadblockDebugInfo();
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            Instances.ForEach(x => DoSafeOperation(x.DeletePreview, $"delete instance slot {x} preview"));
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public virtual void Dispose()
        {
            Instances.ForEach(x => DoSafeOperation(x.Dispose, $"dispose instance slot {x}"));
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Number of {nameof(Instances)}: {Instances.Count}, {nameof(Position)}: {Position}, {nameof(Heading)}: {Heading}, " +
                   $"{nameof(BarrierType)}: {BarrierType} {nameof(VehicleType)}: {VehicleType}";
        }

        /// <inheritdoc />
        public virtual void Spawn()
        {
            if (IsPreviewActive)
                DeletePreview();

            Instances.ForEach(x => x.Spawn());
            CopInstances
                .Select(x => x.GameInstance)
                .ToList()
                .ForEach(x =>
                {
                    Functions.SetPedAsCop(x);
                    Functions.SetCopAsBusy(x, true);
                });
        }

        /// <inheritdoc />
        public void ModifyVehiclePosition(Vector3 newPosition)
        {
            Assert.NotNull(newPosition, "newPosition cannot be null");
            var vehicleSlot = Instances.First(x => x.Type == EntityType.CopVehicle);
            vehicleSlot.Position = newPosition;
        }

        /// <inheritdoc />
        public void Release()
        {
            RoadblockHelpers.ReleaseInstancesToLspdfr(Instances, Vehicle);
        }

        #endregion

        #region Functions

        /// <summary>
        /// Initialize this roadblock slot.
        /// This will create all entities/scenery items of this slot.
        /// </summary>
        protected void Initialize()
        {
            InitializeVehicleSlot();
            InitializeCops();
            InitializeScenery();

            if (!BarrierType.IsNone)
                InitializeBarriers();

            if (_shouldAddLights)
                InitializeLights();
        }

        /// <summary>
        /// Calculate the position for cops which is behind the vehicle.
        /// This calculation is based on the width of the vehicle model.
        /// </summary>
        /// <returns>Returns the position behind the vehicle.</returns>
        protected Vector3 CalculatePositionBehindVehicle()
        {
            var model = VehicleFactory.CreateModel(VehicleType, Position);
            return Position + MathHelper.ConvertHeadingToDirection(Heading) * (model.Dimensions.X + 0.5f);
        }

        /// <summary>
        /// Initialize the cop ped slots.
        /// </summary>
        protected abstract void InitializeCops();

        /// <summary>
        /// Initialize the scenery props of this slot.
        /// </summary>
        protected abstract void InitializeScenery();

        /// <summary>
        /// Initialize the light props of this slots.
        /// </summary>
        protected abstract void InitializeLights();

        /// <summary>
        /// Calculate the heading of the vehicle for this slot.
        /// </summary>
        /// <returns>Returns the heading for the vehicle.</returns>
        protected virtual float CalculateVehicleHeading()
        {
            // because nobody is perfect and we want a natural look on the roadblocks
            // we slightly create an offset in the heading for each created vehicle
            return Heading + Random.Next(90 - VehicleHeadingMaxOffset, 91 + VehicleHeadingMaxOffset);
        }

        private void InitializeVehicleSlot()
        {
            if (VehicleType == VehicleType.None)
                return;

            if (!VehicleModel.IsLoaded)
            {
                Logger.Trace($"Loading vehicle slot model {VehicleModel.Name}");
                VehicleModel.LoadAndWait();
            }

            Instances.Add(new InstanceSlot(EntityType.CopVehicle, Position, CalculateVehicleHeading(),
                (position, heading) => VehicleFactory.CreateWithModel(VehicleModel, position, heading, RecordVehicleCollisions)));
        }

        private void InitializeBarriers()
        {
            // verify if a barrier type is given
            if (BarrierType == BarrierType.None)
                return;

            Logger.Trace("Initializing the roadblock slot barriers");
            var rowPosition = Position + MathHelper.ConvertHeadingToDirection(Heading - 180) * 3f;
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(Heading + 90) * (Lane.Width / 2 - BarrierType.Width / 2);
            var direction = MathHelper.ConvertHeadingToDirection(Heading - 90);
            var barrierTotalWidth = BarrierType.Spacing + BarrierType.Width;
            var totalBarriers = (int)Math.Floor(Lane.Width / barrierTotalWidth);

            Logger.Trace($"Barrier info: lane width {Lane.Width}, type {BarrierType}, width: {BarrierType.Width}, spacing: {BarrierType.Spacing}");
            Logger.Debug($"Creating a total of {totalBarriers} barriers with type {BarrierType} for the roadblock slot");
            for (var i = 0; i < totalBarriers; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.Barrier, startPosition, Heading, CreateBarrier));
                startPosition += direction * barrierTotalWidth;
            }
        }

        private IARInstance<Entity> CreateBarrier(Vector3 position, float heading)
        {
            try
            {
                return BarrierFactory.Create(BarrierType, position, heading);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create barrier of type {BarrierType}, {ex.Message}", ex);
                return null;
            }
        }

        private void DoSafeOperation(Action action, string operation)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to {operation}, {ex.Message}", ex);
            }
        }

        [Conditional("DEBUG")]
        private void DrawRoadblockDebugInfo()
        {
            Logger.Trace("Drawing the roadblock slot debug information within the preview");
            Game.NewSafeFiber(() =>
            {
                var direction = MathHelper.ConvertHeadingToDirection(Heading);
                var position = Position + Vector3.WorldUp * 0.25f;

                while (IsPreviewActive)
                {
                    Game.DrawArrow(position, direction, Rotator.Zero, 2f, Color.Yellow);
                    Game.FiberYield();
                }
            }, "IRoadblockSlot.CreatePreview");
        }

        #endregion
    }
}