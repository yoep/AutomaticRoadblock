namespace AutomaticRoadblocks.Settings
{
    public class ManualPlacementSettings
    {
        /// <summary>
        /// Enable a preview of the roadblock that will be placed (a marker will be shown otherwise)
        /// It's only recommended to turn on this feature on high spec computers
        /// </summary>
        public bool EnablePreview { get; internal set; }
        
        /// <summary>
        /// The distance in front of the player for which the manual roadblock should be placed
        /// This distance is used to find the road closest in front of the player
        /// </summary>
        public float DistanceFromPlayer { get; internal set; }
    }
}