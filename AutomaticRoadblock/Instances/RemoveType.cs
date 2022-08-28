using System.Collections.Generic;

namespace AutomaticRoadblocks.Instances
{
    public class RemoveType
    {
        public static readonly RemoveType ClosestToPlayer = new("Closest");
        public static readonly RemoveType LastPlaced = new("Last placed");
        public static readonly RemoveType All = new("All");

        public static readonly IEnumerable<RemoveType> Values = new[]
        {
            All,
            LastPlaced,
            ClosestToPlayer,
        };

        private RemoveType(string displayText)
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