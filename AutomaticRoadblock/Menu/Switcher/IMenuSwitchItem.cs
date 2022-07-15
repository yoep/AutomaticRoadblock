using RAGENativeUI;

namespace AutomaticRoadblocks.Menu.Switcher
{
    public interface IMenuSwitchItem
    {
        /// <summary>
        /// Get the menu of this menu switch item.
        /// </summary>
        UIMenu Menu { get; }
        
        /// <summary>
        /// Get the type of this menu switcher.
        /// </summary>
        MenuType Type { get; }
        
        /// <summary>
        /// Get the text which needs to be displayed for this item.
        /// </summary>
        string DisplayText { get; }
    }
}