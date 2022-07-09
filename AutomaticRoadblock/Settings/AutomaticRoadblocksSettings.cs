namespace AutomaticRoadblocks.Settings
{
    public class AutomaticRoadblocksSettings
    {
        public bool EnableDuringPursuits { get; internal set; }

        public long DispatchAllowedAfter { get; internal set; }

        public long DispatchInterval { get; internal set; }
        
        public bool SlowTraffic { get; internal set; }
    }
}