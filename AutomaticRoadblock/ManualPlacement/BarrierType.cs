namespace AutomaticRoadblocks.ManualPlacement
{
    public class BarrierType
    {
        public static readonly BarrierType None = new("None");
        public static readonly BarrierType SmallCone = new("Small cone");
        public static readonly BarrierType BigCone = new("Big cone");
        public static readonly BarrierType PoliceDoNotCross = new("Police do not cross");
        
        private BarrierType(string displayText)
        {
            DisplayText = displayText;
        }

        /// <summary>
        /// Verify if this barrier type is the <see cref="None"/> type.
        /// </summary>
        public bool IsNone => this == None;

        /// <summary>
        /// Get the display text for this barrier type.
        /// </summary>
        public string DisplayText { get; }
    }
}