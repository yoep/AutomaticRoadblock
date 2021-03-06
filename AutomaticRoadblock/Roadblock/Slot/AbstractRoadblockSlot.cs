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
    public abstract class AbstractRoadblockSlot : IRoadblockSlot
    {
        protected readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        protected readonly List<InstanceSlot> Instances = new();
        protected readonly Random Random = new();

        protected AbstractRoadblockSlot(Road.Lane lane, BarrierType barrierType, float heading, bool shouldAddLights)
        {
            Assert.NotNull(lane, "lane cannot be null");
            Assert.NotNull(barrierType, "barrierType cannot be null");
            Assert.NotNull(heading, "heading cannot be null");
            Lane = lane;
            BarrierType = barrierType;
            Heading = heading;
            VehicleModel = GetVehicleModel();

            Init(shouldAddLights);
        }

        #region Properties

        /// <inheritdoc />
        public Vector3 Position => Lane.Position;

        /// <inheritdoc />
        public float Heading { get; }

        /// <inheritdoc />
        public Vehicle Vehicle => VehicleInstance?.GameInstance;

        /// <inheritdoc />
        public event RoadblockEvents.RoadblockSlotEvents.RoadblockCopKilled RoadblockCopKilled;

        /// <summary>
        /// Get the lane of this slot.
        /// </summary>
        protected Road.Lane Lane { get; }

        /// <summary>
        /// The barrier type that is used within this slot.
        /// </summary>
        public BarrierType BarrierType { get; }

        /// <summary>
        /// Get the police vehicle model which is used for the slot.
        /// </summary>
        protected Model VehicleModel { get; }

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
        public bool IsPreviewActive => Instances.First().IsPreviewActive;

        /// <inheritdoc />
        public void CreatePreview()
        {
            var game = IoC.Instance.GetInstance<IGame>();

            Instances.ForEach(x => x.CreatePreview());
            game.NewSafeFiber(() =>
            {
                var direction = MathHelper.ConvertHeadingToDirection(Heading);
                var position = Position + Vector3.WorldUp * 0.25f;

                while (IsPreviewActive)
                {
                    Rage.Debug.DrawArrow(position, direction, Rotator.Zero, 2f, Color.Yellow);
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
            return $"{nameof(Instances)}: {Instances.Count},\n" +
                   $"{nameof(Position)}: {Position},\n" +
                   $"{nameof(Heading)}: {Heading}";
        }

        /// <inheritdoc />
        public virtual void Spawn()
        {
            if (IsPreviewActive)
                DeletePreview();

            Instances.ForEach(x => x.Spawn());
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
        public void ReleaseToLspdfr()
        {
            var copPeds = Instances
                .Where(x => x.Type == EntityType.CopPed)
                .ToList();

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

                    x.Dismiss();
                    Functions.SetPedAsCop(x);
                    Functions.SetCopAsBusy(x, false);
                });

            // remove all cop instances so that we don't remove them by accident
            // these instances are now in control of LSPDFR
            Instances.RemoveAll(x => x.Type is EntityType.CopPed or EntityType.CopVehicle);
        }

        #endregion

        #region Functions

        private void Init(bool shouldAddLights)
        {
            InitializeVehicleSlot();
            InitializeCopPeds();
            InitializeScenery();

            if (!BarrierType.IsNone)
                InitializeBarriers();

            if (shouldAddLights)
                InitializeLights();
        }

        protected virtual void InitializeVehicleSlot()
        {
            Assert.NotNull(VehicleModel, "VehicleModel has not been initialized, unable to create vehicle slot");
            var initialPosition = Position;

            // move the vehicle a little bit to the border of the road
            // this should prevent clipping
            switch (Lane.Type)
            {
                case Road.Lane.LaneType.RightLane:
                    initialPosition += MathHelper.ConvertHeadingToDirection(Lane.Heading + 90) * 1.5f;
                    break;
                case Road.Lane.LaneType.LeftLane:
                    initialPosition += MathHelper.ConvertHeadingToDirection(Lane.Heading - 90) * 1.5f;
                    break;
            }

            Instances.Add(new InstanceSlot(EntityType.CopVehicle, initialPosition, Heading + 90,
                (position, heading) => new ARVehicle(VehicleModel, GameUtils.GetOnTheGroundVector(position), heading)));
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
        protected abstract void InitializeCopPeds();

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

        private void InitializeBarriers()
        {
            Logger.Trace("Initializing the roadblock slot barriers");
            var rowPosition = Position + MathHelper.ConvertHeadingToDirection(Heading - 180) * 3f;
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(Heading + 90) * Lane.Width;
            var direction = MathHelper.ConvertHeadingToDirection(Heading - 90);
            var barrierTotalWidth = BarrierType.Spacing + BarrierType.Width;
            var totalBarriers = (int)Math.Ceiling(Lane.Width / barrierTotalWidth);

            Logger.Debug($"Creating a total of {totalBarriers} barriers for the roadblock slot");
            for (var i = 0; i < totalBarriers; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.Scenery, startPosition, Heading, CreateBarrier));
                startPosition += direction * barrierTotalWidth;
            }
        }

        private ARInstance<Entity> CreateBarrier(Vector3 position, float heading)
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

        #endregion
    }
}