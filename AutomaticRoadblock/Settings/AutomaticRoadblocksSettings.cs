namespace AutomaticRoadblocks.Settings
{
    public class AutomaticRoadblocksSettings
    {
        /// <summary>
        /// Verify if automatic roadblocks should be enabled during pursuits (can be changed in-game through the menu)
        /// </summary>
        public bool EnableDuringPursuits { get; internal set; }
        
        /// <summary>
        /// Verify if lights should be used within the roadblocks during nighttime
        /// </summary>
        public bool EnableLights { get; internal set; }

        /// <summary>
        /// The minimum wait time in seconds before the first roadblock may be dispatched when a pursuit is started
        /// </summary>
        public long DispatchAllowedAfter { get; internal set; }

        /// <summary>
        /// The minimum wait time in seconds between roadblocks before a new roadblock may be dispatched
        /// </summary>
        public long DispatchInterval { get; internal set; }

        /// <summary>
        /// The minimum time in seconds before the level can be automatically increased again during a pursuit
        /// </summary>
        public long TimeBetweenAutoLevelIncrements { get; internal set; }

        /// <summary>
        /// Verify if the traffic needs to be slowed down at the roadblock
        /// </summary>
        public bool SlowTraffic { get; internal set; }
    }
}