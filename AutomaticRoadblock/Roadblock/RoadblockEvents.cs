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
    }
}