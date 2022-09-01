using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Vehicles;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualPlacementVehicleTypeComponentItem : IMenuComponent<UIMenuListScrollerItem<VehicleType>>
    {
        private readonly IManualPlacement _manualPlacement;
        private readonly ILocalizer _localizer;

        public ManualPlacementVehicleTypeComponentItem(IManualPlacement manualPlacement, ILocalizer localizer)
        {
            _manualPlacement = manualPlacement;
            _localizer = localizer;

            MenuItem = new UIMenuListScrollerItem<VehicleType>(localizer[LocalizationKey.Vehicle], localizer[LocalizationKey.VehicleDescription],
                VehicleType.Values);
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<VehicleType> MenuItem { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.ManualPlacement;

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
            MenuItem.Formatter = type => _localizer[type.LocalizationKey];
            MenuItem.SelectedItem = _manualPlacement.VehicleType;
            MenuItem.IndexChanged += MenuIndexChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _manualPlacement.VehicleType = MenuItem.SelectedItem;
        }
    }
}