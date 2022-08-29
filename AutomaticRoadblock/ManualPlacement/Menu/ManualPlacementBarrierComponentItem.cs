using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualPlacementBarrierComponentItem : IMenuComponent<UIMenuListScrollerItem<BarrierType>>
    {
        private readonly IManualPlacement _manualPlacement;

        public ManualPlacementBarrierComponentItem(IManualPlacement manualPlacement, ILocalizer localizer)
        {
            _manualPlacement = manualPlacement;

            MenuItem = new UIMenuListScrollerItem<BarrierType>(localizer[LocalizationKey.Barrier], localizer[LocalizationKey.BarrierDescription],
                BarrierType.Values);
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<BarrierType> MenuItem { get; }

        /// <inheritdoc />
        public MenuType Type => MenuType.ManualPlacement;

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
            MenuItem.SelectedItem = _manualPlacement.Barrier;
            MenuItem.IndexChanged += MenuIndexChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _manualPlacement.Barrier = MenuItem.SelectedItem;
        }
    }
}