using System.Collections.Generic;

namespace AutomaticRoadblocks.Localization
{
    public class LocalizationKey
    {
        public static readonly LocalizationKey MenuTitle = new(nameof(MenuTitle), "Automatic Roadblocks");
        public static readonly LocalizationKey MenuSubtitle = new(nameof(MenuSubtitle), "Dispatch roadblocks");

        public static readonly IEnumerable<LocalizationKey> Values = new[]
        {
            MenuTitle,
            MenuSubtitle
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