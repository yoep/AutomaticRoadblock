using System.Collections.Generic;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class PlacementRemoveType
    {
        public static readonly PlacementRemoveType ClosestToPlayer = new("Closest");
        public static readonly PlacementRemoveType LastPlaced = new("Last placed");
        public static readonly PlacementRemoveType All = new("All");

        public static readonly IEnumerable<PlacementRemoveType> Values = new[]
        {
            All,
            LastPlaced,
            ClosestToPlayer,
        };

        private PlacementRemoveType(string displayText)
        {
            DisplayText = displayText;
        }

        /// <summary>
        /// Get the text to display.
        /// </summary>
        public string DisplayText { get; }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}