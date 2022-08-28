using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficConeTypeComponentItem : IMenuComponent<UIMenuListScrollerItem<BarrierType>>
    {
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;

        public RedirectTrafficConeTypeComponentItem(IRedirectTrafficDispatcher redirectTrafficDispatcher)
        {
            _redirectTrafficDispatcher = redirectTrafficDispatcher;
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<BarrierType> MenuItem { get; } =
            new(AutomaticRoadblocksPlugin.Barrier, AutomaticRoadblocksPlugin.BarrierDescription, new List<BarrierType>
            {
                BarrierType.SmallCone,
                BarrierType.SmallConeStriped,
                BarrierType.BigCone,
                BarrierType.BigConeStriped,
                BarrierType.WorkBarrierSmall,
                BarrierType.ConeWithLight
            });

        /// <inheritdoc />
        public MenuType Type => MenuType.RedirectTraffic;

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
            MenuItem.SelectedItem = _redirectTrafficDispatcher.ConeType;
            MenuItem.IndexChanged += MenuIndexChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _redirectTrafficDispatcher.ConeType = MenuItem.SelectedItem;
        }
    }
}