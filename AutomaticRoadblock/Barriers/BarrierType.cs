using System.Collections.Generic;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Barriers
{
    public class BarrierType
    {
        public static readonly BarrierType None = new(LocalizationKey.BarrierTypeNone, 1f, 1f);
        public static readonly BarrierType SmallCone = new(LocalizationKey.BarrierTypeSmallCone, DimensionOf(PropUtils.Models.SmallCone), 0.4f);
        public static readonly BarrierType SmallConeStriped = new(LocalizationKey.BarrierTypeSmallConeStriped, DimensionOf(PropUtils.Models.SmallConeWithStrips), 0.4f);
        public static readonly BarrierType BigCone = new(LocalizationKey.BarrierTypeBigCone, DimensionOf(PropUtils.Models.BigCone), 0.5f);
        public static readonly BarrierType BigConeStriped = new(LocalizationKey.BarrierTypeBigConeStriped, DimensionOf(PropUtils.Models.BigConeWithStrips), 0.5f);
        public static readonly BarrierType PoliceDoNotCross = new(LocalizationKey.BarrierTypePoliceDoNotCross, DimensionOf(PropUtils.Models.PoliceDoNotCross), 0.2f);
        public static readonly BarrierType WorkBarrierLarge = new(LocalizationKey.BarrierTypeWorkBarrierLarge, DimensionOf(PropUtils.Models.WorkBarrierLarge), 0.2f);
        public static readonly BarrierType WorkBarrierSmall = new(LocalizationKey.BarrierTypeWorkBarrierSmall, DimensionOf(PropUtils.Models.WorkBarrierSmall), 0.3f);
        public static readonly BarrierType WorkBarrierSmallWithLight = new(LocalizationKey.BarrierTypeWorkBarrierSmallWithLight, DimensionOf(PropUtils.Models.WorkBarrierSmallWithLight), 0.3f);
        public static readonly BarrierType WorkBarrierWithSign = new(LocalizationKey.BarrierTypeWorkBarrierWithSign, DimensionOf(PropUtils.Models.WorkBarrierAHeadSign), 0.2f);
        public static readonly BarrierType WorkBarrierWithSignLight = new(LocalizationKey.BarrierTypeWorkBarrierWithSignLight, DimensionOf(PropUtils.Models.WorkBarrierAHeadSignLights), 0.2f);
        public static readonly BarrierType WorkBarrierHigh = new(LocalizationKey.BarrierTypeWorkBarrierHigh, DimensionOf(PropUtils.Models.WorkBarrierHigh), 0.2f);
        public static readonly BarrierType BarrelTrafficCatcher = new(LocalizationKey.BarrierTypeBarrelTrafficCatcher, DimensionOf(PropUtils.Models.BarrelTrafficCatcher), 0.5f);
        public static readonly BarrierType ConeWithLight = new(LocalizationKey.BarrierTypeConeWithLight, DimensionOf(PropUtils.Models.ConeWithLight), 0.5f);

        public static readonly IEnumerable<BarrierType> Values = new[]
        {
            None,
            SmallCone,
            SmallConeStriped,
            BigCone,
            BigConeStriped,
            ConeWithLight,
            PoliceDoNotCross,
            WorkBarrierLarge,
            WorkBarrierSmall,
            WorkBarrierSmallWithLight,
            WorkBarrierWithSign,
            WorkBarrierWithSignLight,
            WorkBarrierHigh,
            BarrelTrafficCatcher
        };

        private BarrierType(LocalizationKey localizationKey, float width, float spacing)
        {
            LocalizationKey = localizationKey;
            Width = width;
            Spacing = spacing;
        }

        /// <summary>
        /// Verify if this barrier type is the <see cref="None"/> type.
        /// </summary>
        public bool IsNone => this == None;

        /// <summary>
        /// Get the display localization identifier.
        /// </summary>
        public LocalizationKey LocalizationKey { get; }

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
            return LocalizationKey.DefaultText;
        }

        private static float DimensionOf(Model model)
        {
            return model.Dimensions.X;
        }
    }
}