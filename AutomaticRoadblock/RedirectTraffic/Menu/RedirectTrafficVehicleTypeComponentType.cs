using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Vehicles;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficVehicleTypeComponentType : IMenuComponent<UIMenuListScrollerItem<VehicleType>>
    {
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;

        public RedirectTrafficVehicleTypeComponentType(IRedirectTrafficDispatcher redirectTrafficDispatcher)
        {
            _redirectTrafficDispatcher = redirectTrafficDispatcher;
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<VehicleType> MenuItem { get; } =
            new(AutomaticRoadblocksPlugin.Vehicle, AutomaticRoadblocksPlugin.VehicleDescription, VehicleType.Values);

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
            MenuItem.SelectedItem = _redirectTrafficDispatcher.VehicleType;
            MenuItem.IndexChanged += MenuIndexChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _redirectTrafficDispatcher.VehicleType = MenuItem.SelectedItem;
        }
    }
}