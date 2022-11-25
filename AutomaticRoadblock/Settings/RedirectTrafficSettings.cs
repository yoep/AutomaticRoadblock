namespace AutomaticRoadblocks.Settings
{
    public class RedirectTrafficSettings
    {
        /// <summary>
        /// Enable a preview of the redirect traffic setup that will be placed (a marker will be shown otherwise)
        /// It's only recommended to turn on this feature on high spec computers
        /// </summary>
        public bool EnablePreview { get; internal set; }
        
        /// <summary>
        /// The distance in front of the player for which the traffic redirection should be placed
        /// This distance is used to find the road closest in front of the player
        /// </summary>
        public float DistanceFromPlayer { get; internal set; }
        
        /// <summary>
        /// Set if lights should be added to the traffic redirection during evening/night time
        /// </summary>
        public bool EnableLights { get; internal set; }
        
        /// <summary>
        /// The default cone type to use when placing a traffic redirection
        /// </summary>
        public string DefaultCone { get; internal set; }
    }
}