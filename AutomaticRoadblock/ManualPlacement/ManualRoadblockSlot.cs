using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.ManualPlacement.Factory;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Utils.Type;
using Rage;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class ManualRoadblockSlot : AbstractRoadblockSlot
    {
        public ManualRoadblockSlot(Road.Lane lane, BarrierType barrierType, VehicleType vehicleType, LightSourceType lightSourceType, float heading,
            bool shouldAddLights, bool copsEnabled)
            : base(lane, barrierType, heading, shouldAddLights, false)
        {
            Assert.NotNull(vehicleType, "vehicleType cannot be null");
            Assert.NotNull(lightSourceType, "lightSourceType cannot be null");
            VehicleType = vehicleType;
            LightSourceType = lightSourceType;
            CopsEnabled = copsEnabled;

            Initialize();
        }

        #region Properties

        /// <summary>
        /// The vehicle type of the slot.
        /// </summary>
        public VehicleType VehicleType { get; }

        /// <summary>
        /// The light type of the slot.
        /// </summary>
        public LightSourceType LightSourceType { get; }

        /// <summary>
        /// The indication if cops are added to the slot.
        /// </summary>
        public bool CopsEnabled { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override string ToString()
        {
            return "{" +
                   $"{base.ToString()}, {nameof(VehicleType)}: {VehicleType}, {nameof(LightSourceType)}: {LightSourceType}, {nameof(CopsEnabled)}: {CopsEnabled}" +
                   "}";
        }

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override void InitializeCops()
        {
            if (!CopsEnabled)
                return;

            Instances.Add(new InstanceSlot(EntityType.CopPed, Position, 0f, (position, _) =>
            {
                var cop = PedFactory.CreateCopWeaponsForModel(new ARPed(GetPedModelForVehicle(), position));
                cop.GameInstance?.WarpIntoVehicle(Vehicle, (int)VehicleSeat.Driver);
                return cop;
            }));
        }

        /// <inheritdoc />
        protected override void InitializeScenery()
        {
            // no-op
        }

        /// <inheritdoc />
        protected override void InitializeLights()
        {
            Logger.Trace("Initializing the manual roadblock slot lights");
                Instances.AddRange(LightSourceSlotFactory.Create(LightSourceType, this));
        }

        /// <inheritdoc />
        protected override Model GetVehicleModel()
        {
            return VehicleFactory.Create(VehicleType, Position);
        }

        #endregion
    }
}