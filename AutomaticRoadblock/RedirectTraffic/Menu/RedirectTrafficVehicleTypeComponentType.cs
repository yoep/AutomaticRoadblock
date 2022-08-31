using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Vehicles;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficVehicleTypeComponentType : IMenuComponent<UIMenuListScrollerItem<VehicleType>>
    {
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;
        private readonly ILocalizer _localizer;

        public RedirectTrafficVehicleTypeComponentType(IRedirectTrafficDispatcher redirectTrafficDispatcher, ILocalizer localizer)
        {
            _redirectTrafficDispatcher = redirectTrafficDispatcher;
            _localizer = localizer;

            MenuItem = new UIMenuListScrollerItem<VehicleType>(localizer[LocalizationKey.Vehicle], localizer[LocalizationKey.VehicleDescription],
                VehicleType.Values);
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<VehicleType> MenuItem { get; }

        /// <inheritdoc />
        public MenuType Type => MenuType.RedirectTraffic;

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
            MenuItem.SelectedItem = _redirectTrafficDispatcher.VehicleType;
            MenuItem.IndexChanged += MenuIndexChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _redirectTrafficDispatcher.VehicleType = MenuItem.SelectedItem;
        }
    }
}