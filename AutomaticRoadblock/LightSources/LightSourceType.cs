using System.Collections.Generic;

namespace AutomaticRoadblocks.LightSources
{
    public class LightSourceType
    {
        public static readonly LightSourceType None = new("None", 0f);
        public static readonly LightSourceType Flares = new("Flares", 1f);
        public static readonly LightSourceType Spots = new("Spots", 2f);
        public static readonly LightSourceType Warning = new("Warning", 1f);
        public static readonly LightSourceType BlueStanding = new("Blue", 2f);
        public static readonly LightSourceType RedStanding = new("Red", 2f);

        public static readonly IEnumerable<LightSourceType> Values = new[]
        {
            None,
            Flares,
            Spots,
            Warning,
            BlueStanding,
            RedStanding,
        };

        private LightSourceType(string displayText, float spacing)
        {
            DisplayText = displayText;
            Spacing = spacing;
        }

        /// <summary>
        /// Get the display text of the light source.
        /// </summary>
        public string DisplayText { get; }
        
        /// <summary>
        /// The spacing which should be applied to the light source.
        /// </summary>
        public float Spacing { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return DisplayText;
        }
    }
}