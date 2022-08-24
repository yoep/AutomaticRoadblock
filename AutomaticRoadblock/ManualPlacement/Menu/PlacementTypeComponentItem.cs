using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class PlacementTypeComponentItem : IMenuComponent<UIMenuListScrollerItem<PlacementType>>
    {
        private readonly IManualPlacement _manualPlacement;

        public PlacementTypeComponentItem(IManualPlacement manualPlacement)
        {
            _manualPlacement = manualPlacement;
            Init();
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<PlacementType> MenuItem { get; } =
            new(AutomaticRoadblocksPlugin.BlockLanes, AutomaticRoadblocksPlugin.BlockLanesDescription, PlacementType.Values);

        /// <inheritdoc />
        public MenuType Type => MenuType.MANUAL_PLACEMENT;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            // no-op   
        }

        private void Init()
        {
            MenuItem.SelectedItem = _manualPlacement.PlacementType;
            MenuItem.IndexChanged += MenuIndexChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldindex, int newindex)
        {
            _manualPlacement.PlacementType = MenuItem.SelectedItem;
        }
    }
}