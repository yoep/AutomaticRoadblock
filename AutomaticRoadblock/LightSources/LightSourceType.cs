using System.Collections.Generic;
using AutomaticRoadblocks.Localization;

namespace AutomaticRoadblocks.LightSources
{
    public class LightSourceType
    {
        public static readonly LightSourceType None = new(LocalizationKey.None, 0f);
        public static readonly LightSourceType Flares = new(LocalizationKey.Flares, 1f);
        public static readonly LightSourceType Spots = new(LocalizationKey.Spots, 2f);
        public static readonly LightSourceType Warning = new(LocalizationKey.Warning, 1f);
        public static readonly LightSourceType BlueStanding = new(LocalizationKey.Blue, 2f);
        public static readonly LightSourceType RedStanding = new(LocalizationKey.Red, 2f);

        public static readonly IEnumerable<LightSourceType> Values = new[]
        {
            None,
            Flares,
            Spots,
            Warning,
            BlueStanding,
            RedStanding,
        };

        private LightSourceType(LocalizationKey localizationKey, float spacing)
        {
            LocalizationKey = localizationKey;
            Spacing = spacing;
        }

        /// <summary>
        /// The key to use for displaying the text.
        /// </summary>
        public LocalizationKey LocalizationKey { get; }
        
        /// <summary>
        /// The spacing which should be applied to the light source.
        /// </summary>
        public float Spacing { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return LocalizationKey.DefaultText;
        }
    }
}