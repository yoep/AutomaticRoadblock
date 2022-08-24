using System.Collections.Generic;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class LightSourceType
    {
        public static readonly LightSourceType None = new("None");
        public static readonly LightSourceType Flares = new("Flares");
        public static readonly LightSourceType Spots = new("Spots");

        public static readonly IEnumerable<LightSourceType> Values = new[]
        {
            None,
            Flares,
            Spots
        };

        private LightSourceType(string displayText)
        {
            DisplayText = displayText;
        }

        /// <summary>
        /// Get the display text of the light source.
        /// </summary>
        public string DisplayText { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return DisplayText;
        }
    }
}