using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils.Road;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class ManualRoadblock : AbstractRoadblock
    {
        internal ManualRoadblock(Road road, float targetHeading, BarrierType barrierType, bool limitSpeed, bool addLights)
            : base(road, barrierType, null, targetHeading, limitSpeed, addLights)
        {
        }

        #region IRoadblock

        /// <inheritdoc />
        public override RoadblockLevel Level => RoadblockLevel.None;

        #endregion

        #region Funtions

        /// <inheritdoc />
        protected override IReadOnlyCollection<IRoadblockSlot> CreateRoadblockSlots(IReadOnlyList<Road.Lane> lanesToBlock, bool addLights)
        {
            return lanesToBlock
                .Select(lane => new ManualRoadblockSlot(lane, MainBarrierType, TargetHeading, addLights))
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