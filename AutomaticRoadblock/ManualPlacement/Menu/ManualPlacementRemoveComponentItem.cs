using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualPlacementRemoveComponentItem : IMenuComponent<UIMenuListScrollerItem<RemoveType>>
    {
        private readonly IManualPlacement _manualPlacement;

        public ManualPlacementRemoveComponentItem(IManualPlacement manualPlacement)
        {
            _manualPlacement = manualPlacement;
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<RemoveType> MenuItem { get; } =
            new(AutomaticRoadblocksPlugin.CleanRoadblockPlacement, AutomaticRoadblocksPlugin.CleanRoadblockPlacementDescription, RemoveType.Values);

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