using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Roadblock;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class BarrierComponentItem : IMenuComponent<UIMenuListScrollerItem<BarrierType>>
    {
        private readonly IManualPlacement _manualPlacement;

        public BarrierComponentItem(IManualPlacement manualPlacement)
        {
            _manualPlacement = manualPlacement;
            Init();
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<BarrierType> MenuItem { get; } =
            new(AutomaticRoadblocksPlugin.Barrier, AutomaticRoadblocksPlugin.BarrierDescription, BarrierType.Values);

        /// <inheritdoc />
        public MenuType Type => MenuType.MANUAL_PLACEMENT;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

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