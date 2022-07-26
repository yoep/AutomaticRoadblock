using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class VehicleComponentItem : IMenuComponent<UIMenuListScrollerItem<VehicleType>>
    {
        private readonly IManualPlacement _manualPlacement;

        public VehicleComponentItem(IManualPlacement manualPlacement)
        {
            _manualPlacement = manualPlacement;
            Init();
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<VehicleType> MenuItem { get; } =
            new(AutomaticRoadblocksPlugin.Vehicle, AutomaticRoadblocksPlugin.VehicleDescription, VehicleType.Values);

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
            MenuItem.SelectedItem = _manualPlacement.VehicleType;
            MenuItem.IndexChanged += MenuIndexChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldindex, int newindex)
        {
            _manualPlacement.VehicleType = MenuItem.SelectedItem;
        }
    }
}