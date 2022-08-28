using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Roadblock;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualPlacementBarrierComponentItem : IMenuComponent<UIMenuListScrollerItem<BarrierType>>
    {
        private readonly IManualPlacement _manualPlacement;

        public ManualPlacementBarrierComponentItem(IManualPlacement manualPlacement)
        {
            _manualPlacement = manualPlacement;
            Init();
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<BarrierType> MenuItem { get; } =
            new(AutomaticRoadblocksPlugin.Barrier, AutomaticRoadblocksPlugin.BarrierDescription, BarrierType.Values);

        /// <inheritdoc />
        public MenuType Type => MenuType.ManualPlacement;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            // no-op   
        }

        private void Init()
        {
            MenuItem.SelectedItem = _manualPlacement.Barrier;
            MenuItem.IndexChanged += MenuIndexChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldindex, int newindex)
        {
            _manualPlacement.Barrier = MenuItem.SelectedItem;
        }
    }
}