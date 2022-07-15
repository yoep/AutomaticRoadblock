using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Menu.Switcher;
using RAGENativeUI;

namespace AutomaticRoadblocks.Roadblock.Menu
{
    public class RoadblockMenuSwitchItem : IMenuSwitchItem
    {
        /// <inheritdoc />
        public UIMenu Menu { get; } = new(AutomaticRoadblocksPlugin.MenuTitle, AutomaticRoadblocksPlugin.MenuSubtitle);

        /// <inheritdoc />
        public MenuType Type => MenuType.PURSUIT;

        /// <inheritdoc />
        public string DisplayText => AutomaticRoadblocksPlugin.MenuPursuit;
    }
}