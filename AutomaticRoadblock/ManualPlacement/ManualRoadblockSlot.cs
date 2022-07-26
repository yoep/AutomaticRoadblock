using AutomaticRoadblocks.ManualPlacement.Factory;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class ManualRoadblockSlot : AbstractRoadblockSlot
    {
        public ManualRoadblockSlot(Road.Lane lane, BarrierType barrierType, VehicleType vehicleType, float heading, bool shouldAddLights)
            : base(lane, barrierType, heading, shouldAddLights)
        {
            Assert.NotNull(vehicleType, "vehicleType cannot be null");
            VehicleType = vehicleType;
        }

        #region Properties

        /// <summary>
        /// The vehicle type of the slot.
        /// </summary>
        public VehicleType VehicleType { get; }

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
            // no-op
        }

        /// <inheritdoc />
        protected override Model GetVehicleModel()
        {
            return VehicleFactory.Create(VehicleType, Position);
        }
    }
}