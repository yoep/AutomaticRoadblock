using System.Collections.Generic;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class VehicleType
    {
        public static readonly VehicleType Locale = new("Locale");
        public static readonly VehicleType State = new("State");
        public static readonly VehicleType Fbi = new("FBI");
        public static readonly VehicleType Swat = new("Swat");

        public static readonly IEnumerable<VehicleType> Values = new[]
        {
            Locale,
            State,
            Fbi,
            Swat
        };

        private VehicleType(string displayText)
        {
            DisplayText = displayText;
        }

        /// <summary>
        /// Get the display text of the vehicle type.
        /// </summary>
        public string DisplayText { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return DisplayText;
        }
    }
}