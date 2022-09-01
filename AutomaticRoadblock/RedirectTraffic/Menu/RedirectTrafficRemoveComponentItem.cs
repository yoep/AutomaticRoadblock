using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficRemoveComponentItem : IMenuComponent<UIMenuListScrollerItem<RemoveType>>
    {
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;

        public RedirectTrafficRemoveComponentItem(IRedirectTrafficDispatcher redirectTrafficDispatcher, ILocalizer localizer)
        {
            _redirectTrafficDispatcher = redirectTrafficDispatcher;

            MenuItem = new UIMenuListScrollerItem<RemoveType>(localizer[LocalizationKey.CleanRoadblockPlacement],
                localizer[LocalizationKey.CleanRoadblockPlacementDescription], RemoveType.Values);
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<RemoveType> MenuItem { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.RedirectTraffic;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _redirectTrafficDispatcher.RemoveTrafficRedirects(MenuItem.SelectedItem);
        }
    }
}