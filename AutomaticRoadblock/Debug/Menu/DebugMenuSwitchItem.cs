using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Menu.Switcher;
using RAGENativeUI;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugMenuSwitchItem : IMenuSwitchItem
    {
        /// <inheritdoc />
        public UIMenu Menu { get; } = new(AutomaticRoadblocksPlugin.MenuTitle, AutomaticRoadblocksPlugin.MenuSubtitle);

        /// <inheritdoc />
        public MenuType Type => MenuType.Debug;

        /// <inheritdoc />
        public string DisplayText => AutomaticRoadblocksPlugin.MenuDebug;
    }
}