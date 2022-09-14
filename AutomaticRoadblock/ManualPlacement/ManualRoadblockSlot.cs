using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Vehicles;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class ManualRoadblockSlot : AbstractRoadblockSlot
    {
        public ManualRoadblockSlot(Road.Lane lane, BarrierModel barrierType, VehicleType vehicleType, LightSourceType lightSourceType, float heading,
            bool shouldAddLights, bool copsEnabled, float offset)
            : base(lane, barrierType, vehicleType, heading, shouldAddLights, false, offset)
        {
            Assert.NotNull(lightSourceType, "lightSourceType cannot be null");
            LightSourceType = lightSourceType;
            CopsEnabled = copsEnabled;

            Initialize();
        }

        #region Properties

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
            if (!CopsEnabled || VehicleType == VehicleType.None)
                return;

            Instances.Add(new InstanceSlot(EEntityType.CopPed, Position, 0f, (position, heading) =>
            {
                var cop = PedFactory.CreateCopWeaponsForModel(PedFactory.CreateCopForVehicle(VehicleModel, position, heading));
                cop.GameInstance?.WarpIntoVehicle(Vehicle, (int)EVehicleSeat.Driver);
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

        #endregion
    }
}