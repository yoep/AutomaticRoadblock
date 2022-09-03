using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Menu
{
    public interface IMenuComponent<out TMenuType> where TMenuType : UIMenuItem
    {
        /// <summary>
        /// Get the menu item to register at nativeUI.
        /// </summary>
        TMenuType MenuItem { get; }
        
        /// <summary>
        /// Get the type of the menu item.
        /// </summary>
        EMenuType Type { get; }

        /// <summary>
        /// Get if the menu component is automatically closed when selected in the menu.
        /// </summary>
        bool IsAutoClosed { get; }

        /// <summary>
        /// Is triggered when the component menu item has been selected.
        /// </summary>
        /// <param name="sender">The menu that activated the menu item.</param>
        void OnMenuActivation(IMenu sender);
    }
}