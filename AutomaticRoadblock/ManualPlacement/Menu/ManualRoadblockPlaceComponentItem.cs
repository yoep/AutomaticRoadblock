using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Utils;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualRoadblockPlaceComponentItem : IMenuComponent<UIMenuItem>
    {
        private readonly IManualPlacement _manualPlacement;
        private readonly ISettingsManager _settingsManager;

        public ManualRoadblockPlaceComponentItem(IManualPlacement manualPlacement, ILocalizer localizer, ISettingsManager settingsManager)
        {
            _manualPlacement = manualPlacement;
            _settingsManager = settingsManager;

            MenuItem = new UIMenuItem(localizer[LocalizationKey.Place], localizer[LocalizationKey.PlaceDescription]);
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.ManualPlacement;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _manualPlacement.PlaceRoadblock(GameUtils.PlayerPosition +
                                            MathHelper.ConvertHeadingToDirection(GameUtils.PlayerHeading) *
                                            _settingsManager.ManualPlacementSettings.DistanceFromPlayer);
        }
    }
}