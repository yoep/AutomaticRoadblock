namespace AutomaticRoadblocks
{
    public static class AutomaticRoadblocksPlugin
    {
        public const string Name = "Automatic Roadblocks";

        public const string MenuDebug = "Debug";
        public const string RoadInfo = "Road info" + DebugIndicatorText;
        public const string RoadPreview = "Road preview" + DebugIndicatorText;
        public const string RoadPreviewDescription = "Create a visual of the road information";
        public const string RoadPreviewClosest = "Closest road";
        public const string RoadPreviewNearby = "Nearby roads";
        public const string RoadPreviewRemove = "Remove preview" + DebugIndicatorText;
        public const string ZoneInfo = "Zone info " + DebugIndicatorText;
        public const string EndCallout = "End current callout " + DebugIndicatorText;
        public const string DispatchPreview = "Dispatch preview" + DebugIndicatorText;
        public const string DispatchPreviewDescription = "Preview a roadblock with the position calculated or at the current location of the player";
        public const string DispatchPreviewCalculateType = "Calculate";
        public const string DispatchPreviewCurrentLocationType = "Current";
        public const string DispatchSpawn = "Dispatch spawn" + DebugIndicatorText;
        public const string DispatchSpawnDescription = "Dispatch a roadblock";
        public const string CleanAllRoadblocks = "Clean all roadblocks" + DebugIndicatorText;
        public const string StartPursuit = "Start pursuit now" + DebugIndicatorText;
        public const string ForcePursuitOnFoot = "Force On-Foot" + DebugIndicatorText;
        public const string ForcePursuitOnFootDescription = "Force the suspects in the pursuit to leave the vehicle";
        public const string EndPursuit = "End pursuit now" + DebugIndicatorText;
        public const string DeploySpikeStrip = "Deploy spike strip " + DebugIndicatorText;
        public const string DeploySpikeStripDescription = "Deploy a spike strip on the nearby road";
        public const string RemoveSpikeStrip = "Remove spike strip " + DebugIndicatorText;

        private const string DebugIndicatorText = " [DEBUG]";
    }
}