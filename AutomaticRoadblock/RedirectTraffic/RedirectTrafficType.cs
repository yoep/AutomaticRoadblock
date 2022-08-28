using System.Collections.Generic;

namespace AutomaticRoadblocks.RedirectTraffic
{
    public class RedirectTrafficType
    {
        public static readonly RedirectTrafficType Shoulder = new("Shoulder");
        public static readonly RedirectTrafficType Lane = new("Lane");

        public static readonly IEnumerable<RedirectTrafficType> Values = new[]
        {
            Lane, 
            Shoulder
        };

        private RedirectTrafficType(string displayText)
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