using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficPlaceComponentItem : IMenuComponent<UIMenuItem>
    {
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;

        public RedirectTrafficPlaceComponentItem(IRedirectTrafficDispatcher redirectTrafficDispatcher)
        {
            _redirectTrafficDispatcher = redirectTrafficDispatcher;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.RedirectTraffic, AutomaticRoadblocksPlugin.RedirectTrafficDescription);

        /// <inheritdoc />
        public MenuType Type => MenuType.RedirectTraffic;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _redirectTrafficDispatcher.Dispatch();
        }
    }
}