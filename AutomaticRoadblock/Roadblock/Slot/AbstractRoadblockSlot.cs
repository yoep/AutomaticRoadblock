using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Roadblock.Factory;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Utils.Type;
using LSPD_First_Response.Mod.API;
using Rage;

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
        protected readonly List<InstanceSlot> Instances = new();

        private readonly bool _shouldAddLights;

        protected AbstractRoadblockSlot(Road.Lane lane, BarrierType barrierType, float heading, bool shouldAddLights, bool recordVehicleCollisions)
        {
            Assert.NotNull(lane, "lane cannot be null");
            Assert.NotNull(barrierType, "barrierType cannot be null");
            Assert.NotNull(heading, "heading cannot be null");
            Lane = lane;
            BarrierType = barrierType;
            Heading = heading;
            RecordVehicleCollisions = recordVehicleCollisions;
            _shouldAddLights = shouldAddLights;
        }

        #region Properties

        /// <inheritdoc />
        public Vector3 Position => Lane.Position;

        /// <inheritdoc />
        public float Heading { get; }

        /// <inheritdoc />
        public Vehicle Vehicle => VehicleInstance?.GameInstance;

        /// <inheritdoc />
        /// <remarks>This field is only available after the <see cref="Initialize"/> method is called.</remarks>
        public Model VehicleModel { get; private set; }

        /// <inheritdoc />
        public event RoadblockEvents.RoadblockSlotEvents.RoadblockCopKilled RoadblockCopKilled;

        /// <inheritdoc />
        public Road.Lane Lane { get; }

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
            var game = IoC.Instance.GetInstance<IGame>();

            Logger.Debug($"Creating a total of {Instances.Count} instances for the roadblock slot preview");
            Logger.Trace($"Roadblock slot instances: \n{string.Join("\n", Instances.Select(x => x.ToString()).ToList())}");
            Instances.ForEach(x => x.CreatePreview());
            Logger.Trace("Drawing the roadblock slot information within the preview");
            game.NewSafeFiber(() =>
            {
                var direction = MathHelper.ConvertHeadingToDirection(Heading);
                var position = Position + Vector3.WorldUp * 0.25f;

                while (IsPreviewActive)
                {
                    game.DrawArrow(position, direction, Rotator.Zero, 2f, Color.Yellow);
                    game.FiberYield();
                }
            }, "IRoadblockSlot.CreatePreview");
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            Instances.ForEach(x => x.DeletePreview());
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public virtual void Dispose()
        {
            Instances.ForEach(x => x.Dispose());
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Number of {nameof(Instances)}: {Instances.Count}, {nameof(Position)}: {Position}, {nameof(Heading)}: {Heading}, " +
                   $"{nameof(BarrierType)}: {BarrierType} {nameof(VehicleModel)}: {VehicleModel.Name}";
        }

        /// <inheritdoc />
        public virtual void Spawn()
        {
            if (IsPreviewActive)
                DeletePreview();

            Instances.ForEach(x => x.Spawn());

            // verify that the vehicle was spawned
            if (Vehicle != null)
                Vehicle.IsSirenOn = true;

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
        public void ReleaseToLspdfr()
        {
            var copPeds = Instances
                .Where(x => x.Type == EntityType.CopPed)
                .ToList();
            
            // release the instances before giving them to LSPDFR
            // this should prevent accidental override of attributes set by LSPDFR
            Instances
                .Where(x => x.Type is EntityType.CopPed or EntityType.CopVehicle)
                .Select(x => x.Instance)
                .ToList()
                .ForEach(x => x.Release());

            Logger.Trace($"Releasing a total of {copPeds.Count} to LSPDFR");
            copPeds
                .Select(x => x.Instance)
                .Select(x => x.GameInstance)
                .Select(x => (Ped)x)
                .ToList()
                .ForEach(x =>
                {
                    // make sure the ped is the vehicle or at least entering it
                    if (!x.IsInVehicle(Vehicle, true))
                        x.Tasks.EnterVehicle(Vehicle, (int)VehicleSeat.Any);

                    Functions.SetPedAsCop(x);
                    Functions.SetCopAsBusy(x, false);
                });

            // remove all cop instances so that we don't remove them by accident
            // these instances are now in control of LSPDFR
            Instances.RemoveAll(x => x.Type is EntityType.CopPed or EntityType.CopVehicle);
        }

        #endregion

        #region Functions

        /// <summary>
        /// Initialize this roadblock slot.
        /// This will create all entities/scenery items of this slot.
        /// </summary>
        protected void Initialize()
        {
            // get the vehicle model and make sure it's loaded into memory
            VehicleModel = GetVehicleModel();
            VehicleModel.Load();

            InitializeVehicleSlot();
            InitializeCops();
            InitializeScenery();

            if (!BarrierType.IsNone)
                InitializeBarriers();

            if (_shouldAddLights)
                InitializeLights();
        }

        protected Vector3 GetPositionBehindVehicle()
        {
            return Position + MathHelper.ConvertHeadingToDirection(Heading) * 3f;
        }

        /// <summary>
        /// Get a police cop ped model for the current vehicle model.
        /// </summary>
        /// <returns>Returns a cop ped model.</returns>
        protected Model GetPedModelForVehicle()
        {
            var model = VehicleModel.Name;

            if (ModelUtils.IsBike(model))
            {
                return ModelUtils.Peds.GetPoliceBikeCop();
            }

            if (ModelUtils.Vehicles.CityVehicleModels.Contains(model) || ModelUtils.Vehicles.CountyVehicleModels.Contains(model) ||
                ModelUtils.Vehicles.StateVehicleModels.Contains(model))
            {
                return ModelUtils.Peds.GetLocalCop(Position);
            }

            return ModelUtils.Vehicles.FbiVehicleModels.Contains(model)
                ? ModelUtils.Peds.GetPoliceFbiCop()
                : ModelUtils.Peds.GetPoliceSwatCop();
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
        /// Get the vehicle model of the slot.
        /// </summary>
        /// <returns>Returns the vehicle model that should be used for this slot.</returns>
        protected abstract Model GetVehicleModel();

        /// <summary>
        /// Invoked the event that a cop from this slot has been killed.
        /// </summary>
        protected void InvokedCopHasBeenKilled()
        {
            RoadblockCopKilled?.Invoke(this);
        }

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
            Assert.NotNull(VehicleModel, "VehicleModel has not been initialized, unable to create vehicle slot");
            Instances.Add(new InstanceSlot(EntityType.CopVehicle, Position, CalculateVehicleHeading(),
                (position, heading) => new ARVehicle(VehicleModel, GameUtils.GetOnTheGroundPosition(position), heading, RecordVehicleCollisions)));
        }

        private void InitializeBarriers()
        {
            Logger.Trace("Initializing the roadblock slot barriers");
            var rowPosition = Position + MathHelper.ConvertHeadingToDirection(Heading - 180) * 3f;
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(Heading + 90) * (Lane.Width / 2 - BarrierType.Width / 2);
            var direction = MathHelper.ConvertHeadingToDirection(Heading - 90);
            var barrierTotalWidth = BarrierType.Spacing + BarrierType.Width;
            var totalBarriers = (int)Math.Floor(Lane.Width / barrierTotalWidth);

            Logger.Debug($"Creating a total of {totalBarriers} barriers with type {BarrierType} for the roadblock slot");
            for (var i = 0; i < totalBarriers; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.Barrier, GameUtils.GetOnTheGroundPosition(startPosition), Heading, CreateBarrier));
                startPosition += direction * barrierTotalWidth;
            }
        }

        private IARInstance<Entity> CreateBarrier(Vector3 position, float heading)
        {
            try
            {
                return BarrierFactory.Create(BarrierType, GameUtils.GetOnTheGroundPosition(position), heading);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create barrier of type {BarrierType}, {ex.Message}", ex);
                return null;
            }
        }

        #endregion
    }
}