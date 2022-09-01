using System.Collections.Generic;

namespace AutomaticRoadblocks.Localization
{
    public class LocalizationKey
    {
        #region Menu

        public static readonly LocalizationKey MenuMode = new(nameof(MenuMode), "MenuMode");
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
        public static readonly LocalizationKey Offset = new(nameof(Offset), "Offset");
        public static readonly LocalizationKey OffsetDescription = new(nameof(OffsetDescription), "The offset of the placement in regards to the vehicle node");

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
        public static readonly LocalizationKey RedirectTrafficEnableRedirectionArrow = new(nameof(RedirectTrafficEnableRedirectionArrow), "Redirect arrow");

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

        #region VehicleType

        public static readonly LocalizationKey VehicleTypeLocal = new(nameof(VehicleTypeLocal), "Local");
        public static readonly LocalizationKey VehicleTypeState = new(nameof(VehicleTypeState), "State");
        public static readonly LocalizationKey VehicleTypeFbi = new(nameof(VehicleTypeFbi), "FBI");
        public static readonly LocalizationKey VehicleTypeSwat = new(nameof(VehicleTypeSwat), "Swat");
        public static readonly LocalizationKey VehicleTypeTransporter = new(nameof(VehicleTypeTransporter), "Transporter");
        public static readonly LocalizationKey VehicleTypeNone = new(nameof(VehicleTypeNone), "None");

        #endregion

        #region BarrierType
        
        public static readonly LocalizationKey BarrierTypeNone = new(nameof(BarrierTypeNone), "None");
        public static readonly LocalizationKey BarrierTypeSmallCone = new(nameof(BarrierTypeSmallCone), "Small cone");
        public static readonly LocalizationKey BarrierTypeSmallConeStriped = new(nameof(BarrierTypeSmallConeStriped), "Small cone striped");
        public static readonly LocalizationKey BarrierTypeBigCone = new(nameof(BarrierTypeBigCone), "Big cone");
        public static readonly LocalizationKey BarrierTypeBigConeStriped = new(nameof(BarrierTypeBigConeStriped), "Big cone striped");
        public static readonly LocalizationKey BarrierTypePoliceDoNotCross = new(nameof(BarrierTypePoliceDoNotCross), "Police do not cross");
        public static readonly LocalizationKey BarrierTypeWorkBarrierLarge = new(nameof(BarrierTypeWorkBarrierLarge), "Work barrier large");
        public static readonly LocalizationKey BarrierTypeWorkBarrierSmall = new(nameof(BarrierTypeWorkBarrierSmall), "Work barrier small");
        public static readonly LocalizationKey BarrierTypeWorkBarrierSmallWithLight = new(nameof(BarrierTypeWorkBarrierSmallWithLight), "Work barrier small light");
        public static readonly LocalizationKey BarrierTypeWorkBarrierWithSign = new(nameof(BarrierTypeWorkBarrierWithSign), "Work ahead sign");
        public static readonly LocalizationKey BarrierTypeWorkBarrierWithSignLight = new(nameof(BarrierTypeWorkBarrierWithSignLight), "Work ahead sign lights");
        public static readonly LocalizationKey BarrierTypeWorkBarrierHigh = new(nameof(BarrierTypeWorkBarrierHigh), "High barrier");
        public static readonly LocalizationKey BarrierTypeBarrelTrafficCatcher = new(nameof(BarrierTypeBarrelTrafficCatcher), "Barrel");
        public static readonly LocalizationKey BarrierTypeConeWithLight = new(nameof(BarrierTypeConeWithLight), "Cone light");

        #endregion

        public static readonly IEnumerable<LocalizationKey> Values = new[]
        {
            Barrier,
            BarrierDescription,
            BarrierTypeBarrelTrafficCatcher,
            BarrierTypeBigCone,
            BarrierTypeBigConeStriped,
            BarrierTypeConeWithLight,
            BarrierTypeNone,
            BarrierTypePoliceDoNotCross,
            BarrierTypeSmallCone,
            BarrierTypeSmallConeStriped,
            BarrierTypeWorkBarrierHigh,
            BarrierTypeWorkBarrierLarge,
            BarrierTypeWorkBarrierSmall,
            BarrierTypeWorkBarrierWithSign,
            BarrierTypeWorkBarrierWithSignLight,
            BlockLanes,
            BlockLanesDescription,
            CleanRoadblockPlacement,
            CleanRoadblockPlacementDescription,
            DispatchNow,
            DispatchNowDescription,
            EnableAutoPursuitLevelIncrease,
            EnableAutoPursuitLevelIncreaseDescription,
            EnableCops,
            EnableCopsDescription,
            EnableDuringPursuit,
            EnableDuringPursuitDescription,
            LightSource,
            LightSourceDescription,
            MenuManualPlacement,
            MenuMode,
            MenuPursuit,
            MenuRedirectTraffic,
            MenuSubtitle,
            MenuTitle,
            Offset,
            OffsetDescription,
            Place,
            PlaceDescription,
            PursuitLevel,
            PursuitLevelDescription,
            RedirectTraffic,
            RedirectTrafficConeDistance,
            RedirectTrafficConeDistanceDescription,
            RedirectTrafficDescription,
            RedirectTrafficEnableRedirectionArrow,
            RedirectTrafficType,
            RedirectTrafficTypeDescription,
            RoadblockDispatchedAt,
            RoadblockDispatchedAt,
            RoadblockHasBeenBypassed,
            RoadblockHasBeenHit,
            RoadblockInstanceCreationFailed,
            RoadblockNoPursuitActive,
            SpeedLimit,
            SpeedLimitDescription,
            Vehicle,
            VehicleDescription,
            VehicleTypeFbi,
            VehicleTypeLocal,
            VehicleTypeNone,
            VehicleTypeState,
            VehicleTypeSwat,
            VehicleTypeTransporter,
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