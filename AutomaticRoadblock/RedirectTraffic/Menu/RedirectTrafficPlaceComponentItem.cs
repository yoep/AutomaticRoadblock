using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficPlaceComponentItem : IMenuComponent<UIMenuItem>
    {
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;

        public RedirectTrafficPlaceComponentItem(IRedirectTrafficDispatcher redirectTrafficDispatcher, ILocalizer localizer)
        {
            _redirectTrafficDispatcher = redirectTrafficDispatcher;

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
            _redirectTrafficDispatcher.DispatchRedirection();
        }
    }
}