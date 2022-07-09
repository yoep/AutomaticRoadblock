namespace AutomaticRoadblocks.Roadblock
{
    public static class RoadblockEvents
    {
        /// <summary>
        /// Invoked when the roadblock state changes.
        /// </summary>
        public delegate void RoadblockStateChanged(RoadblockState newState);
    }
}