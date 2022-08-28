using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class LightComponentItem : IMenuComponent<UIMenuListScrollerItem<LightSourceType>>
    {
        private readonly IManualPlacement _manualPlacement;

        public LightComponentItem(IManualPlacement manualPlacement)
        {
            _manualPlacement = manualPlacement;
            Init();
        }
        
        /// <inheritdoc />
        public UIMenuListScrollerItem<LightSourceType> MenuItem { get; } =
            new(AutomaticRoadblocksPlugin.LightSource, AutomaticRoadblocksPlugin.LightSourceDescription, LightSourceType.Values);

        /// <inheritdoc />
        public MenuType Type => MenuType.ManualPlacement;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            // no-op   
        }

        private void Init()
        {
            MenuItem.SelectedItem = _manualPlacement.LightSourceType;
            MenuItem.IndexChanged += MenuIndexChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _manualPlacement.LightSourceType = MenuItem.SelectedItem;
        }
    }
}