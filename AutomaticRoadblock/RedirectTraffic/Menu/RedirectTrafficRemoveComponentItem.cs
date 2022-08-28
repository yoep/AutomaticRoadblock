using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficRemoveComponentItem : IMenuComponent<UIMenuListScrollerItem<RemoveType>>
    {
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;

        public RedirectTrafficRemoveComponentItem(IRedirectTrafficDispatcher redirectTrafficDispatcher)
        {
            _redirectTrafficDispatcher = redirectTrafficDispatcher;
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<RemoveType> MenuItem { get; } =
            new(AutomaticRoadblocksPlugin.CleanRoadblockPlacement, AutomaticRoadblocksPlugin.CleanRoadblockPlacementDescription, RemoveType.Values);

        /// <inheritdoc />
        public MenuType Type => MenuType.RedirectTraffic;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _redirectTrafficDispatcher.RemoveTrafficRedirects(MenuItem.SelectedItem);
        }
    }
}