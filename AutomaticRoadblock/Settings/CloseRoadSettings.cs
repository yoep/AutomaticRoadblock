namespace AutomaticRoadblocks.Settings
{
    public class CloseRoadSettings
    {
        /// <summary>
        /// The maximum distance from the player the road may be closed.
        /// </summary>
        public float MaxDistanceFromPlayer { get; internal set; }
        
        /// <summary>
        /// The barrier to use when closing the road.
        /// </summary>
        public string Barrier { get; internal set; }
    }
}