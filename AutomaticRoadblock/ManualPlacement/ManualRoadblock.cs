using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils.Road;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class ManualRoadblock : AbstractRoadblock
    {

        internal ManualRoadblock(Road road, BarrierType barrierType, VehicleType vehicleType, float targetHeading, bool limitSpeed, bool addLights)
            : base(road, barrierType, targetHeading, limitSpeed, addLights)
        {
            Assert.NotNull(vehicleType, "vehicleType cannot be null");
            VehicleType = vehicleType;
            
            Initialize();
        }

        #region Properties
        
        /// <summary>
        /// The vehicle type used within the roadblock.
        /// </summary>
        public VehicleType VehicleType { get; }

        #endregion

        #region IRoadblock

        /// <inheritdoc />
        public override RoadblockLevel Level => RoadblockLevel.None;

        #endregion

        #region Funtions

        /// <inheritdoc />
        protected override IReadOnlyCollection<IRoadblockSlot> CreateRoadblockSlots(IReadOnlyList<Road.Lane> lanesToBlock)
        {
            return lanesToBlock
                .Select(lane => new ManualRoadblockSlot(lane, MainBarrierType, VehicleType, TargetHeading, IsLightsEnabled))
                .ToList();
        }

        /// <inheritdoc />
        protected override void InitializeScenery()
        {
        }

        /// <inheritdoc />
        protected override void InitializeLights()
        {
        }

        #endregion
    }
}