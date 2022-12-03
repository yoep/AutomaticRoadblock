using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Settings;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualRoadblockPlaceComponentItem : IMenuComponent<UIMenuItem>
    {
        private readonly IManualPlacement _manualPlacement;
        private readonly IGame _game;
        private readonly ISettingsManager _settingsManager;

        public ManualRoadblockPlaceComponentItem(IManualPlacement manualPlacement, ILocalizer localizer, IGame game, ISettingsManager settingsManager)
        {
            _manualPlacement = manualPlacement;
            _game = game;
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
            _manualPlacement.PlaceRoadblock(_game.PlayerPosition +
                                            MathHelper.ConvertHeadingToDirection(_game.PlayerHeading) *
                                            _settingsManager.ManualPlacementSettings.DistanceFromPlayer);
        }
    }
}