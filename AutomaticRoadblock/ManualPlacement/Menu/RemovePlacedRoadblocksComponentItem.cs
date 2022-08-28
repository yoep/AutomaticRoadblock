using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class RemovePlacedRoadblocksComponentItem : IMenuComponent<UIMenuListScrollerItem<PlacementRemoveType>>
    {
        private readonly IManualPlacement _manualPlacement;

        public RemovePlacedRoadblocksComponentItem(IManualPlacement manualPlacement)
        {
            _manualPlacement = manualPlacement;
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<PlacementRemoveType> MenuItem { get; } =
            new(AutomaticRoadblocksPlugin.CleanRoadblockPlacement, AutomaticRoadblocksPlugin.CleanRoadblockPlacementDescription, PlacementRemoveType.Values);

        /// <inheritdoc />
        public MenuType Type => MenuType.ManualPlacement;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _manualPlacement.RemoveRoadblocks(MenuItem.SelectedItem);
        }
    }
}