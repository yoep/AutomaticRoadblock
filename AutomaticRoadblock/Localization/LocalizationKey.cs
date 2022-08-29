using System.Collections.Generic;

namespace AutomaticRoadblocks.Localization
{
    public class LocalizationKey
    {
        public static readonly LocalizationKey MenuTitle = new(nameof(MenuTitle), "Automatic Roadblocks");
        public static readonly LocalizationKey MenuSubtitle = new(nameof(MenuSubtitle), "Dispatch roadblocks");
        public static readonly LocalizationKey MenuPursuit = new(nameof(MenuPursuit), "Pursuit");
        public static readonly LocalizationKey MenuManualPlacement = new(nameof(MenuManualPlacement), "Manual placement");
        public static readonly LocalizationKey MenuRedirectTraffic = new(nameof(MenuRedirectTraffic), "Redirect traffic");
        
        public static readonly LocalizationKey EnableDuringPursuit = new(nameof(EnableDuringPursuit), "Automatic");
        public static readonly LocalizationKey EnableDuringPursuitDescription = new(nameof(EnableDuringPursuitDescription), "Enable automatic roadblock dispatching during a pursuit");
        public static readonly LocalizationKey EnableAutoPursuitLevelIncrease = new(nameof(EnableAutoPursuitLevelIncrease), "Level increase");
        public static readonly LocalizationKey EnableAutoPursuitLevelIncreaseDescription = new(nameof(EnableAutoPursuitLevelIncrease), "Enable automatic level increases during a pursuit");
        public static readonly LocalizationKey DispatchNow = new(nameof(EnableAutoPursuitLevelIncrease), "Dispatch now");
        public static readonly LocalizationKey DispatchNowDescription = new(nameof(DispatchNowDescription), "Dispatch a roadblock now for the current pursuit");
        public static readonly LocalizationKey PursuitLevel = new(nameof(PursuitLevel), "Level");
        public static readonly LocalizationKey PursuitLevelDescription = new(nameof(PursuitLevelDescription), "The pursuit level which determines the roadblock type");

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
            PursuitLevelDescription
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