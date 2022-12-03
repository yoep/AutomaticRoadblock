using System.Collections.Generic;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class PlacementType
    {
        public static readonly PlacementType ClosestToPlayer = new("Closest");
        public static readonly PlacementType SameDirectionAsRoad = new("Same direction");
        public static readonly PlacementType OppositeDirectionOfRoad = new("Opposite direction");
        public static readonly PlacementType All = new("All");

        public static readonly IEnumerable<PlacementType> Values = new[]
        {
            ClosestToPlayer,
            SameDirectionAsRoad,
            OppositeDirectionOfRoad,
            All
        };

        private PlacementType(string displayText)
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