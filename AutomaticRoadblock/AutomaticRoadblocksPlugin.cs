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
        public const string EndPursuit = "End pursuit now" + DebugIndicatorText;

        public const string RedirectTraffic = "Place redirection";
        public const string RedirectTrafficDescription = "Place a redirection at the current location";
        public const string RedirectTrafficConeDistance = "Cone distance";
        public const string RedirectTrafficConeDistanceDescription = "The distance along the road to which cones should be placed";
        public const string RedirectTrafficType = "Redirect";
        public const string RedirectTrafficTypeDescription = "Place a traffic redirection on";

        private const string DebugIndicatorText = " [DEBUG]";
    }
}