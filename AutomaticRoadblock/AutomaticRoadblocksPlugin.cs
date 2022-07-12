namespace AutomaticRoadblocks
{
    public static class AutomaticRoadblocksPlugin
    {
        public const string Name = "Automatic Roadblocks";

        public const string RoadInfo = "Road info" + DebugIndicatorText;
        public const string RoadPreview = "Road preview" + DebugIndicatorText;
        public const string RoadPreviewRemove = "Remove road preview" + DebugIndicatorText;
        public const string NearbyRoadsPreview = "Nearby roads preview" + DebugIndicatorText;
        public const string NearbyRoadsPreviewRemove = "Remove nearby roads preview" + DebugIndicatorText;
        public const string ZoneInfo = "Zone info " + DebugIndicatorText;
        public const string EndCallout = "End current callout " + DebugIndicatorText;

        public const string EnableDuringPursuit = "Enabled";
        public const string EnableDuringPursuitDescription = "Enable automatic roadblock dispatching during pursuit";
        public const string DispatchNow = "Dispatch now";
        public const string DispatchNowDescription = "Dispatch a roadblock now for the current pursuit";
        public const string DispatchPreview = "Dispatch preview" + DebugIndicatorText;
        public const string DispatchPreviewDescription = "Preview or spawn a roadblock as it would during a pursuit";
        public const string DispatchPreviewPreviewType = "Preview";
        public const string DispatchPreviewSpawnType = "Spawn";
        public const string PursuitLevel = "Level";
        public const string PursuitLevelDescription = "The pursuit level which determines the roadblock type";
        public const string CleanAllRoadblocks = "Clean all roadblocks" + DebugIndicatorText;
        public const string StartPursuit = "Start pursuit now" + DebugIndicatorText;
        public const string EndPursuit = "End pursuit now" + DebugIndicatorText;

        private const string DebugIndicatorText = " [DEBUG]";
    }
}