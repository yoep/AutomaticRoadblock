using AutomaticRoadblocks.Roadblock.Slot;

namespace AutomaticRoadblocks.Roadblock
{
    public static class RoadblockEvents
    {
        /// <summary>
        /// Invoked when the roadblock state changes.
        /// </summary>
        /// <param name="roadblock">The roadblock of which the state changed.</param>
        /// <param name="newState">The new state of the roadblock.</param>
        public delegate void RoadblockStateChanged(IRoadblock roadblock, RoadblockState newState);

        /// <summary>
        /// Invoked when the suspect has killed a cop from the roadblock.
        /// </summary>
        /// <param name="roadblock">The roadblock of which the cop was killed.</param>
        public delegate void RoadblockCopKilled(IRoadblock roadblock);

        /// <summary>
        /// Internal events which can be triggered by a slot.
        /// </summary>
        public static class RoadblockSlotEvents
        {
            /// <summary>
            /// Invoked when the suspect has killed a cop from the roadblock slot.
            /// </summary>
            public delegate void RoadblockCopKilled(IRoadblockSlot slot);
        }
    }
}