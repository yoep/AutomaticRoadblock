using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficLaneTypeComponentItem : IMenuComponent<UIMenuListScrollerItem<RedirectTrafficType>>
    {
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;

        public RedirectTrafficLaneTypeComponentItem(IRedirectTrafficDispatcher redirectTrafficDispatcher, ILocalizer localizer)
        {
            _redirectTrafficDispatcher = redirectTrafficDispatcher;

            MenuItem = new UIMenuListScrollerItem<RedirectTrafficType>(localizer[LocalizationKey.RedirectTrafficType],
                localizer[LocalizationKey.RedirectTrafficTypeDescription], RedirectTrafficType.Values);
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<RedirectTrafficType> MenuItem { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.RedirectTraffic;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            // no-op   
        }

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            MenuItem.SelectedItem = _redirectTrafficDispatcher.Type;
            MenuItem.IndexChanged += MenuIndexChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _redirectTrafficDispatcher.Type = MenuItem.SelectedItem;
        }
    }
}