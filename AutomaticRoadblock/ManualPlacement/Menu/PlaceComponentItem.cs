using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class PlaceComponentItem : IMenuComponent<UIMenuItem>
    {
        private readonly IManualPlacement _manualPlacement;

        public PlaceComponentItem(IManualPlacement manualPlacement)
        {
            _manualPlacement = manualPlacement;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.Place, AutomaticRoadblocksPlugin.PlaceDescription);

        /// <inheritdoc />
        public MenuType Type => MenuType.MANUAL_PLACEMENT;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _manualPlacement.PlaceRoadblock();
        }
    }
}