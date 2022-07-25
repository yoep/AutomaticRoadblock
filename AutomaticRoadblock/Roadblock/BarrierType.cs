using System.Collections.Generic;

namespace AutomaticRoadblocks.Roadblock
{
    public class BarrierType
    {
        public static readonly BarrierType None = new("None", 1f, 1f);
        public static readonly BarrierType SmallCone = new("Small cone", 0.5f, 1.5f);
        public static readonly BarrierType BigCone = new("Big cone", 0.5f, 1.5f);
        public static readonly BarrierType PoliceDoNotCross = new("Police do not cross", 1.5f, 1f);
        public static readonly BarrierType WorkBarrierLarge = new("Work barrier large", 1.5f, 1f);
        public static readonly BarrierType WorkBarrierSmall = new("Work barrier small", 1f, 0.5f);
        public static readonly BarrierType WorkBarrierWithSign = new("Work ahead sign", 1.5f, 1f);
        public static readonly BarrierType WorkBarrierWithSignLight = new("Work ahead sign lights", 1.5f, 1f);

        public static readonly IEnumerable<BarrierType> Values = new[]
        {
            None,
            SmallCone,
            BigCone,
            PoliceDoNotCross,
            WorkBarrierLarge,
            WorkBarrierSmall,
            WorkBarrierWithSign,
            WorkBarrierWithSignLight
        };

        private BarrierType(string displayText, float width, float spacing)
        {
            DisplayText = displayText;
            Width = width;
            Spacing = spacing;
        }

        /// <summary>
        /// Verify if this barrier type is the <see cref="None"/> type.
        /// </summary>
        public bool IsNone => this == None;

        /// <summary>
        /// Get the display text for this barrier type.
        /// </summary>
        public string DisplayText { get; }

        /// <summary>
        /// Get the recommended spacing distance for the barrier.
        /// </summary>
        public float Spacing { get; }

        /// <summary>
        /// Get the width of the barrier type.
        /// </summary>
        public float Width { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return DisplayText;
        }
    }
}