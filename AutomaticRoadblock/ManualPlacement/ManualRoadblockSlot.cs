using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.ManualPlacement.Factory;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class ManualRoadblockSlot : AbstractRoadblockSlot
    {
        public ManualRoadblockSlot(Road.Lane lane, BarrierType barrierType, VehicleType vehicleType, LightSourceType lightSourceType, float heading,
            bool shouldAddLights)
            : base(lane, barrierType, heading, shouldAddLights)
        {
            Assert.NotNull(vehicleType, "vehicleType cannot be null");
            Assert.NotNull(lightSourceType, "lightSourceType cannot be null");
            VehicleType = vehicleType;
            LightSourceType = lightSourceType;

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

        #endregion

        /// <inheritdoc />
        protected override void InitializeCopPeds()
        {
            // no-op
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
            if (LightSourceType == LightSourceType.Flares)
            {
                Instances.AddRange(LightSourceSlotFactory.CreateFlares(this));
            }
        }

        /// <inheritdoc />
        protected override Model GetVehicleModel()
        {
            return VehicleFactory.Create(VehicleType, Position);
        }
    }
}