using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualRoadblockPlaceComponentItem : IMenuComponent<UIMenuItem>
    {
        private readonly IManualPlacement _manualPlacement;

        public ManualRoadblockPlaceComponentItem(IManualPlacement manualPlacement)
        {
            _manualPlacement = manualPlacement;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.Place, AutomaticRoadblocksPlugin.PlaceDescription);

        /// <inheritdoc />
        public MenuType Type => MenuType.ManualPlacement;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _manualPlacement.PlaceRoadblock();
        }
    }
}