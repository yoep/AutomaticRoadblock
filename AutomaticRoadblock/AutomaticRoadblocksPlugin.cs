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
        public const string MenuRedirectTraffic = "Redirect traffic";

        public const string RoadInfo = "Road info" + DebugIndicatorText;
        public const string RoadPreview = "Road preview" + DebugIndicatorText;
        public const string RoadPreviewDescription = "Create a visual of the road information";
        public const string RoadPreviewClosest = "Closest road";
        public const string RoadPreviewNearby = "Nearby roads";
        public const string RoadPreviewRemove = "Remove preview" + DebugIndicatorText;
        public const string ZoneInfo = "Zone info " + DebugIndicatorText;
        public const string EndCallout = "End current callout " + DebugIndicatorText;

        public const string EnableDuringPursuit = "Automatic";
        public const string EnableDuringPursuitDescription = "Enable automatic roadblock dispatching during a pursuit";
        public const string EnableAutoPursuitLevelIncrease = "Level increase";
        public const string EnableAutoPursuitLevelIncreaseDescription = "Enable automatic level increases during a pursuit";
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
        public const string EnableCops = "Enable cops";
        public const string EnableCopsDescription = "Set if cops should be spawned with the roadblock";
        public const string SpeedLimit = "Slow traffic";
        public const string SpeedLimitDescription = "Slow the traffic around the roadblock";
        public const string Vehicle = "Vehicle";
        public const string VehicleDescription = "The vehicle type to use";
        public const string LightSource = "Lights";
        public const string LightSourceDescription = "The lights type to use";
        public const string BlockLanes = "Block lanes";
        public const string BlockLanesDescription = "The lanes which should be blocked";
        public const string CleanRoadblockPlacement = "Remove";
        public const string CleanRoadblockPlacementDescription = "Remove one or more placed roadblocks";
        
        public const string RedirectTraffic = "Place redirection";
        public const string RedirectTrafficDescription = "Place a redirection at the current location";
        public const string RedirectTrafficConeDistance = "Cone distance";
        public const string RedirectTrafficConeDistanceDescription = "The distance along the road to which cones should be placed";

        private const string DebugIndicatorText = " [DEBUG]";
    }
}