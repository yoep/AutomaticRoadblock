using System.Collections.Generic;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    public class BarrierType
    {
        public static readonly BarrierType None = new("None", 1f, 1f);
        public static readonly BarrierType SmallCone = new("Small cone", DimensionOf(PropUtils.Models.SmallCone), 0.35f);
        public static readonly BarrierType SmallConeStriped = new("Small cone striped", DimensionOf(PropUtils.Models.SmallConeWithStrips), 0.35f);
        public static readonly BarrierType BigCone = new("Big cone", DimensionOf(PropUtils.Models.BigCone), 0.45f);
        public static readonly BarrierType BigConeStriped = new("Big cone striped", DimensionOf(PropUtils.Models.BigConeWithStrips), 0.45f);
        public static readonly BarrierType PoliceDoNotCross = new("Police do not cross", DimensionOf(PropUtils.Models.PoliceDoNotCross), 0.2f);
        public static readonly BarrierType WorkBarrierLarge = new("Work barrier large", DimensionOf(PropUtils.Models.WorkBarrierLarge), 0.2f);
        public static readonly BarrierType WorkBarrierSmall = new("Work barrier small", DimensionOf(PropUtils.Models.WorkBarrierSmall), 0.3f);
        public static readonly BarrierType WorkBarrierWithSign = new("Work ahead sign", DimensionOf(PropUtils.Models.WorkBarrierAHeadSign), 0.2f);
        public static readonly BarrierType WorkBarrierWithSignLight = new("Work ahead sign lights", DimensionOf(PropUtils.Models.WorkBarrierAHeadSignLights), 0.2f);
        public static readonly BarrierType WorkBarrierHigh = new("High barrier", DimensionOf(PropUtils.Models.WorkBarrierHigh), 0.2f);
        public static readonly BarrierType BarrelTrafficCatcher = new("Barrel", DimensionOf(PropUtils.Models.BarrelTrafficCatcher), 0.5f);

        public static readonly IEnumerable<BarrierType> Values = new[]
        {
            None,
            SmallCone,
            SmallConeStriped,
            BigCone,
            BigConeStriped,
            PoliceDoNotCross,
            WorkBarrierLarge,
            WorkBarrierSmall,
            WorkBarrierWithSign,
            WorkBarrierWithSignLight,
            WorkBarrierHigh,
            BarrelTrafficCatcher
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

        private static float DimensionOf(Model model)
        {
            return model.Dimensions.X;
        }
    }
}