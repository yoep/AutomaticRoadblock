using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Menu.Switcher;
using RAGENativeUI;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficMenuSwitchItem : IMenuSwitchItem
    {
        /// <inheritdoc />
        public UIMenu Menu { get; } = new(AutomaticRoadblocksPlugin.MenuTitle, AutomaticRoadblocksPlugin.MenuSubtitle);

        /// <inheritdoc />
        public MenuType Type => MenuType.RedirectTraffic;

        /// <inheritdoc />
        public string DisplayText => AutomaticRoadblocksPlugin.MenuRedirectTraffic;
    }
}