using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Vehicles;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    /// <summary>
    /// Abstract implementation of the <see cref="IRoadblockSlot"/>.
    /// This slot defines the entities and scenery items used within the <see cref="IRoadblock"/>.
    /// </summary>
    /// <remarks>Make sure that the <see cref="Initialize"/> method is called within the constructor after all properties/fields are set for the slot.</remarks>
    public abstract class AbstractRoadblockSlot : IRoadblockSlot
    {
        protected const int VehicleHeadingMaxOffset = 10;
        protected static readonly Random Random = new();

        protected readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        protected readonly IGame Game = IoC.Instance.GetInstance<IGame>();

        private readonly bool _shouldAddLights;

        protected AbstractRoadblockSlot(Road.Lane lane, BarrierModel mainBarrier, BarrierModel secondaryBarrier, EBackupUnit backupType, float heading,
            bool shouldAddLights, bool recordVehicleCollisions, float offset = 0f)
        {
            Assert.NotNull(lane, "lane cannot be null");
            Assert.NotNull(mainBarrier, "barrierType cannot be null");
            Assert.NotNull(backupType, "backupType cannot be null");
            Assert.NotNull(heading, "heading cannot be null");
            Lane = lane;
            MainBarrier = mainBarrier;
            SecondaryBarrier = secondaryBarrier;
            BackupType = backupType;
            Heading = heading;
            RecordVehicleCollisions = recordVehicleCollisions;
            Offset = offset;
            _shouldAddLights = shouldAddLights;

            if (backupType != EBackupUnit.None)
            {
                VehicleModel = LspdfrDataHelper.RetrieveVehicleModel(backupType, OffsetPosition);

                var loadout = LspdfrDataHelper.RetrieveLoadout(BackupType, Position);
                NumberOfCops = Random.Next(loadout.NumPeds.Min, loadout.NumPeds.Max + 1);
            }
        }

        #region Properties

        /// <inheritdoc />
        public Vector3 Position => Lane.Position;

        /// <inheritdoc />
        public Vector3 OffsetPosition => Position + MathHelper.ConvertHeadingToDirection(Heading) * Offset;

        /// <inheritdoc />
        public float Heading { get; }

        /// <inheritdoc />
        public List<InstanceSlot> Instances { get; } = new();

        /// <inheritdoc />
        public Vehicle Vehicle => VehicleInstance?.GameInstance;

        /// <inheritdoc />
        public Model VehicleModel { get; }

        /// <inheritdoc />
        public Road.Lane Lane { get; }

        /// <inheritdoc />
        public IEnumerable<ARPed> Cops => Instances
            .Where(x => x.Type == EEntityType.CopPed)
            .Select(x => x.Instance)
            .Select(x => (ARPed)x)
            .ToList();

        /// <summary>
        /// The main barrier that is used within this slot as first row.
        /// </summary>
        public BarrierModel MainBarrier { get; }

        /// <summary>
        /// The secondary barrier that is used within this slot as second row.
        /// </summary>
        public BarrierModel SecondaryBarrier { get; }

        public EBackupUnit BackupType { get; }

        /// <summary>
        /// The indication if the spawned vehicle should record collisions.
        /// </summary>
        protected bool RecordVehicleCollisions { get; }

        /// <summary>
        /// The offset of the position in regards to the node.
        /// </summary>
        protected float Offset { get; }

        /// <summary>
        /// The total number of cops for this slot.
        /// </summary>
        protected int NumberOfCops { get; set; }

        /// <summary>
        /// Get the AR vehicle instance of this slot.
        /// </summary>
        protected ARVehicle VehicleInstance => Instances
            .Where(x => x.Type == EEntityType.CopVehicle)
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
                    .Where(x => x.Type == EEntityType.CopPed)
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

            if (Instances.Any(x => x.State == EInstanceState.Error))
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
            return
                $"Number of {nameof(Instances)}: {Instances.Count}, {nameof(Position)}: {Position}, {nameof(OffsetPosition)}: {OffsetPosition}, {nameof(Heading)}: {Heading}, " +
                $"{nameof(MainBarrier)}: {MainBarrier}, {nameof(SecondaryBarrier)}: {SecondaryBarrier}, {nameof(BackupType)}: {BackupType}";
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
            var vehicleSlot = Instances.First(x => x.Type == EEntityType.CopVehicle);
            vehicleSlot.Position = newPosition;
        }

        /// <inheritdoc />
        public virtual void Release()
        {
            RoadblockHelpers.ReleaseInstancesToLspdfr(this);
        }

        /// <inheritdoc />
        public void WarpInVehicle()
        {
            CopInstances.ToList()
                .ForEach(x => x.WarpIntoVehicle(Vehicle, Vehicle.Driver == null ? EVehicleSeat.Driver : EVehicleSeat.Any));
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

            if (!MainBarrier.IsNone)
                InitializeBarriers(MainBarrier, 3f);

            if (!SecondaryBarrier.IsNone)
                InitializeBarriers(SecondaryBarrier, -3f);

            if (_shouldAddLights)
                InitializeLights();
        }

        /// <summary>
        /// Calculate the position for cops which is behind the vehicle.
        /// This calculation is based on the width of the vehicle model.
        /// </summary>
        /// <returns>Returns the position behind the vehicle.</returns>
        protected virtual Vector3 CalculatePositionBehindVehicle()
        {
            return OffsetPosition + MathHelper.ConvertHeadingToDirection(Heading) * (VehicleModel.Dimensions.X + 0.5f);
        }

        /// <summary>
        /// Calculate the facing heading of the cop instances.
        /// </summary>
        /// <returns>Returns the heading for the cop peds.</returns>
        protected virtual float CalculateCopHeading()
        {
            return Heading - 180;
        }

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

        /// <summary>
        /// Calculate the position of the vehicle for this slot.
        /// </summary>
        /// <returns>Returns the position for the vehicle placement.</returns>
        protected virtual Vector3 CalculateVehiclePosition()
        {
            return OffsetPosition;
        }

        private void InitializeVehicleSlot()
        {
            if (BackupType == EBackupUnit.None)
                return;

            if (!VehicleModel.IsLoaded)
            {
                Logger.Trace($"Loading vehicle slot model {VehicleModel.Name}");
                VehicleModel.LoadAndWait();
            }

            Instances.Add(new InstanceSlot(EEntityType.CopVehicle, CalculateVehiclePosition(), CalculateVehicleHeading(),
                (position, heading) => new ARVehicle(VehicleModel, GameUtils.GetOnTheGroundPosition(position), heading, RecordVehicleCollisions)));
        }

        private void InitializeCops()
        {
            var pedSpawnPosition = CalculatePositionBehindVehicle();
            var pedHeading = CalculateCopHeading();

            for (var i = 0; i < NumberOfCops; i++)
            {
                Instances.Add(new InstanceSlot(EEntityType.CopPed, GameUtils.GetOnTheGroundPosition(pedSpawnPosition), pedHeading,
                    (position, heading) =>
                        PedFactory.ToInstance(LspdfrDataHelper.RetrieveCop(BackupType, position), position, heading)));
                pedSpawnPosition += MathHelper.ConvertHeadingToDirection(Heading + 90) * 1.5f;
            }
        }

        private void InitializeBarriers(BarrierModel barrierModel, float roadOffset)
        {
            Logger.Trace($"Initializing roadblock slot barriers for {{{barrierModel}}} with offset {roadOffset}");
            var rowPosition = OffsetPosition + MathHelper.ConvertHeadingToDirection(Heading - 180) * roadOffset;
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(Heading + 90) * (Lane.Width / 2 - barrierModel.Width / 2);
            var direction = MathHelper.ConvertHeadingToDirection(Heading - 90);
            var barrierTotalWidth = barrierModel.Spacing + barrierModel.Width;
            var totalBarriers = (int)Math.Floor(Lane.Width / barrierTotalWidth);

            Logger.Trace($"Barrier info: lane width {Lane.Width}, type {barrierModel}, width: {barrierModel.Width}, spacing: {barrierModel.Spacing}");
            Logger.Debug($"Creating a total of {totalBarriers} barriers with type {barrierModel} for the roadblock slot");
            for (var i = 0; i < totalBarriers; i++)
            {
                Instances.Add(new InstanceSlot(EEntityType.Barrier, startPosition, Heading,
                    (position, heading) => CreateBarrier(barrierModel, position, heading)));
                startPosition += direction * barrierTotalWidth;
            }
        }

        private IARInstance<Entity> CreateBarrier(BarrierModel barrierModel, Vector3 position, float heading)
        {
            try
            {
                return BarrierFactory.Create(barrierModel, position, heading);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create barrier of type {barrierModel}, {ex.Message}", ex);
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
                var position = OffsetPosition + Vector3.WorldUp * 0.25f;

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