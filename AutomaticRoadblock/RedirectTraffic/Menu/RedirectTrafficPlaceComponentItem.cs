using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Settings;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficPlaceComponentItem : IMenuComponent<UIMenuItem>
    {
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;
        private readonly IGame _game;
        private readonly ISettingsManager _settingsManager;

        public RedirectTrafficPlaceComponentItem(IRedirectTrafficDispatcher redirectTrafficDispatcher, ILocalizer localizer, IGame game, ISettingsManager settingsManager)
        {
            _redirectTrafficDispatcher = redirectTrafficDispatcher;
            _game = game;
            _settingsManager = settingsManager;

            MenuItem = new UIMenuItem(localizer[LocalizationKey.RedirectTraffic], localizer[LocalizationKey.RedirectTrafficDescription]);
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.RedirectTraffic;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _redirectTrafficDispatcher.DispatchRedirection(_game.PlayerPosition + MathHelper.ConvertHeadingToDirection(_game.PlayerHeading) *
                _settingsManager.RedirectTrafficSettings.DistanceFromPlayer);
        }
    }
}