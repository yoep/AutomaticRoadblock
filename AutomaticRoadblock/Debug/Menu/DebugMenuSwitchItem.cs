using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Menu.Switcher;
using RAGENativeUI;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugMenuSwitchItem : IMenuSwitchItem
    {
        /// <inheritdoc />
        public UIMenu Menu { get; } = new(IoC.Instance.GetInstance<ILocalizer>()[LocalizationKey.MenuTitle],
            "~b~" + IoC.Instance.GetInstance<ILocalizer>()[LocalizationKey.MenuSubtitle]);

        /// <inheritdoc />
        public MenuType Type => MenuType.Debug;

        /// <inheritdoc />
        public string DisplayText => AutomaticRoadblocksPlugin.MenuDebug;
    }
}