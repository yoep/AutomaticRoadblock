using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Utils;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficPlaceComponentItem : IMenuComponent<UIMenuItem>
    {
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;
        private readonly ISettingsManager _settingsManager;

        public RedirectTrafficPlaceComponentItem(IRedirectTrafficDispatcher redirectTrafficDispatcher, ILocalizer localizer, ISettingsManager settingsManager)
        {
            _redirectTrafficDispatcher = redirectTrafficDispatcher;
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
            _redirectTrafficDispatcher.DispatchRedirection(GameUtils.PlayerPosition + MathHelper.ConvertHeadingToDirection(GameUtils.PlayerHeading) *
                _settingsManager.RedirectTrafficSettings.DistanceFromPlayer);
        }
    }
}