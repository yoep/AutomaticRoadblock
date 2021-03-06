namespace AutomaticRoadblocks
{
    public static class AutomaticRoadblocksPlugin
    {
        public const string Name = "Automatic Roadblocks";
        
        public const string MenuTitle = "Automatic Roadblocks";
        public const string MenuSubtitle = "~b~Dispatch roadblocks";
        public const string MenuDebug = "Debug";
        public const string MenuPursuit = "Pursuit";
        public const string MenuManualPlacement = "Manual placement";

        public const string RoadInfo = "Road info" + DebugIndicatorText;
        public const string RoadPreview = "Road preview" + DebugIndicatorText;
        public const string RoadPreviewDescription = "Create a visual of the road information";
        public const string RoadPreviewClosest = "Closest road";
        public const string RoadPreviewNearby = "Nearby roads";
        public const string RoadPreviewRemove = "Remove preview" + DebugIndicatorText;
        public const string ZoneInfo = "Zone info " + DebugIndicatorText;
        public const string EndCallout = "End current callout " + DebugIndicatorText;

        public const string EnableDuringPursuit = "Enabled";
        public const string EnableDuringPursuitDescription = "Enable automatic roadblock dispatching during pursuit";
        public const string DispatchNow = "Dispatch now";
        public const string DispatchNowDescription = "Dispatch a roadblock now for the current pursuit";
        public const string DispatchPreview = "Dispatch preview" + DebugIndicatorText;
        public const string DispatchPreviewDescription = "Preview a roadblock with the position calculated or at the current location of the player";
        public const string DispatchPreviewCalculateType = "Calculate";
        public const string DispatchPreviewCurrentLocationType = "Current";
        public const string DispatchSpawn = "Dispatch spawn" + DebugIndicatorText;
        public const string DispatchSpawnDescription = "Dispatch a roadblock";
        public const string PursuitLevel = "Level";
        public const string PursuitLevelDescription = "The pursuit level which determines the roadblock type";
        public const string CleanAllRoadblocks = "Clean all roadblocks" + DebugIndicatorText;
        public const string StartPursuit = "Start pursuit now" + DebugIndicatorText;
        public const string EndPursuit = "End pursuit now" + DebugIndicatorText;
        
        public const string Place = "Place";
        public const string PlaceDescription = "Place a roadblock at the current hightlighted location";
        public const string Barrier = "Barrier";
        public const string BarrierDescription = "The barrier type to use";
        public const string Vehicle = "Vehicle";
        public const string VehicleDescription = "The vehicle type to use";

        private const string DebugIndicatorText = " [DEBUG]";
    }
}