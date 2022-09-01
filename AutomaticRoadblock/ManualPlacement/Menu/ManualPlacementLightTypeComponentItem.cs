using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualPlacementLightTypeComponentItem : IMenuComponent<UIMenuListScrollerItem<LightSourceType>>
    {
        private readonly IManualPlacement _manualPlacement;

        public ManualPlacementLightTypeComponentItem(IManualPlacement manualPlacement, ILocalizer localizer)
        {
            _manualPlacement = manualPlacement;

            MenuItem = new UIMenuListScrollerItem<LightSourceType>(localizer[LocalizationKey.LightSource], localizer[LocalizationKey.LightSourceDescription],
                LightSourceType.Values);
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<LightSourceType> MenuItem { get; }

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
            MenuItem.SelectedItem = _manualPlacement.LightSourceType;
            MenuItem.IndexChanged += MenuIndexChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _manualPlacement.LightSourceType = MenuItem.SelectedItem;
        }
    }
}