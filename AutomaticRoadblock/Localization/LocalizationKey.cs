using System.Collections.Generic;

namespace AutomaticRoadblocks.Localization
{
    public class LocalizationKey
    {
        #region Menu

        public static readonly LocalizationKey MenuTitle = new(nameof(MenuTitle), "Automatic Roadblocks");
        public static readonly LocalizationKey MenuSubtitle = new(nameof(MenuSubtitle), "Dispatch roadblocks");
        public static readonly LocalizationKey MenuPursuit = new(nameof(MenuPursuit), "Pursuit");
        public static readonly LocalizationKey MenuManualPlacement = new(nameof(MenuManualPlacement), "Manual placement");
        public static readonly LocalizationKey MenuRedirectTraffic = new(nameof(MenuRedirectTraffic), "Redirect traffic");

        #endregion

        #region Pursuit

        public static readonly LocalizationKey EnableDuringPursuit = new(nameof(EnableDuringPursuit), "Automatic");


        public static readonly LocalizationKey EnableDuringPursuitDescription =
            new(nameof(EnableDuringPursuitDescription), "Enable automatic roadblock dispatching during a pursuit");

        public static readonly LocalizationKey EnableAutoPursuitLevelIncrease = new(nameof(EnableAutoPursuitLevelIncrease), "Level increase");

        public static readonly LocalizationKey EnableAutoPursuitLevelIncreaseDescription =
            new(nameof(EnableAutoPursuitLevelIncrease), "Enable automatic level increases during a pursuit");

        public static readonly LocalizationKey DispatchNow = new(nameof(DispatchNow), "Dispatch now");
        public static readonly LocalizationKey DispatchNowDescription = new(nameof(DispatchNowDescription), "Dispatch a roadblock now for the current pursuit");
        public static readonly LocalizationKey PursuitLevel = new(nameof(PursuitLevel), "Level");

        public static readonly LocalizationKey PursuitLevelDescription =
            new(nameof(PursuitLevelDescription), "The pursuit level which determines the roadblock type");

        #endregion

        #region Manual placement

        public static readonly LocalizationKey Place = new(nameof(Place), "Place");
        public static readonly LocalizationKey PlaceDescription = new(nameof(PlaceDescription), "Place a roadblock at the current highlighted location");
        public static readonly LocalizationKey Barrier = new(nameof(Barrier), "Barrier");
        public static readonly LocalizationKey BarrierDescription = new(nameof(BarrierDescription), "The barrier type to use");
        public static readonly LocalizationKey EnableCops = new(nameof(EnableCops), "Enable cops");
        public static readonly LocalizationKey EnableCopsDescription = new(nameof(EnableCopsDescription), "Check if cops should be spawned with the roadblock");
        public static readonly LocalizationKey SpeedLimit = new(nameof(SpeedLimit), "Slow traffic");
        public static readonly LocalizationKey SpeedLimitDescription = new(nameof(SpeedLimitDescription), "Slow the traffic around the roadblock");
        public static readonly LocalizationKey Vehicle = new(nameof(Vehicle), "Vehicle");
        public static readonly LocalizationKey VehicleDescription = new(nameof(VehicleDescription), "The vehicle type to use");
        public static readonly LocalizationKey LightSource = new(nameof(LightSource), "Lights");
        public static readonly LocalizationKey LightSourceDescription = new(nameof(LightSourceDescription), "The lights type to use");
        public static readonly LocalizationKey BlockLanes = new(nameof(BlockLanes), "Block lanes");
        public static readonly LocalizationKey BlockLanesDescription = new(nameof(BlockLanesDescription), "The lanes which should be blocked");
        public static readonly LocalizationKey CleanRoadblockPlacement = new(nameof(CleanRoadblockPlacement), "Remove");

        public static readonly LocalizationKey CleanRoadblockPlacementDescription =
            new(nameof(CleanRoadblockPlacementDescription), "Remove one or more placed roadblocks based on the selected criteria");

        #endregion

        #region Redirect traffic

        public static readonly LocalizationKey RedirectTraffic = new(nameof(RedirectTraffic), "Place redirection");

        public static readonly LocalizationKey RedirectTrafficDescription =
            new(nameof(RedirectTrafficDescription), "Place a traffic redirection at the highlighted location");

        public static readonly LocalizationKey RedirectTrafficConeDistance = new(nameof(RedirectTrafficConeDistance), "Cone distance");

        public static readonly LocalizationKey RedirectTrafficConeDistanceDescription =
            new(nameof(RedirectTrafficConeDistanceDescription), "The distance along the road to which cones should be placed");

        public static readonly LocalizationKey RedirectTrafficType = new(nameof(RedirectTrafficType), "Redirect");
        public static readonly LocalizationKey RedirectTrafficTypeDescription = new(nameof(RedirectTrafficTypeDescription), "Place a traffic redirection on");

        #endregion

        #region Roadblock deployment

        public static readonly LocalizationKey RoadblockDispatchedAt = new(nameof(RoadblockDispatchedAt), "Dispatching ~b~roadblock~s~ at {0}");
        public static readonly LocalizationKey RoadblockHasBeenHit = new(nameof(RoadblockHasBeenHit), "~g~Roadblock has been hit");
        public static readonly LocalizationKey RoadblockHasBeenBypassed = new(nameof(RoadblockHasBeenBypassed), "~r~Roadblock has been bypassed");

        public static readonly LocalizationKey RoadblockNoPursuitActive = new(nameof(RoadblockNoPursuitActive),
            "~r~Unable to create pursuit roadblock preview, no active vehicle in pursuit or player not in vehicle");
        
        public static readonly LocalizationKey RoadblockInstanceCreationFailed = new(nameof(RoadblockInstanceCreationFailed),
            "~r~One or more instance(s) failed to spawn, please check the logs for more info");

        #endregion

        public static readonly IEnumerable<LocalizationKey> Values = new[]
        {
            MenuTitle,
            MenuSubtitle,
            MenuPursuit,
            MenuManualPlacement,
            MenuRedirectTraffic,
            EnableDuringPursuit,
            EnableDuringPursuitDescription,
            EnableAutoPursuitLevelIncrease,
            EnableAutoPursuitLevelIncreaseDescription,
            DispatchNow,
            DispatchNowDescription,
            PursuitLevel,
            PursuitLevelDescription,
            Place,
            PlaceDescription,
            Barrier,
            BarrierDescription,
            EnableCops,
            EnableCopsDescription,
            SpeedLimit,
            SpeedLimitDescription,
            Vehicle,
            VehicleDescription,
            LightSource,
            LightSourceDescription,
            BlockLanes,
            BlockLanesDescription,
            CleanRoadblockPlacement,
            CleanRoadblockPlacementDescription,
            RedirectTraffic,
            RedirectTrafficDescription,
            RedirectTrafficConeDistance,
            RedirectTrafficConeDistanceDescription,
            RedirectTrafficType,
            RedirectTrafficTypeDescription,
            RoadblockDispatchedAt,
            RoadblockDispatchedAt,
            RoadblockHasBeenHit,
            RoadblockHasBeenBypassed,
            RoadblockNoPursuitActive,
            RoadblockInstanceCreationFailed
        };

        private LocalizationKey(string identifier, string defaultText)
        {
            Identifier = identifier;
            DefaultText = defaultText;
        }

        /// <summary>
        /// The key identifier.
        /// </summary>
        internal string Identifier { get; }

        /// <summary>
        /// The fallback text when the identifier could not be found.
        /// </summary>
        internal string DefaultText { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return DefaultText;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LocalizationKey)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Identifier != null ? Identifier.GetHashCode() : 0);
        }

        protected bool Equals(LocalizationKey other)
        {
            return Identifier == other.Identifier;
        }
    }
}