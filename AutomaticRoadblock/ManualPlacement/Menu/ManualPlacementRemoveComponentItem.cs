using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualPlacementRemoveComponentItem : IMenuComponent<UIMenuListScrollerItem<RemoveType>>
    {
        private readonly IManualPlacement _manualPlacement;

        public ManualPlacementRemoveComponentItem(IManualPlacement manualPlacement, ILocalizer localizer)
        {
            _manualPlacement = manualPlacement;

            MenuItem = new UIMenuListScrollerItem<RemoveType>(localizer[LocalizationKey.CleanRoadblockPlacement],
                localizer[LocalizationKey.CleanRoadblockPlacementDescription], RemoveType.Values);
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<RemoveType> MenuItem { get; }

        /// <inheritdoc />
        public MenuType Type => MenuType.ManualPlacement;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _manualPlacement.RemoveRoadblocks(MenuItem.SelectedItem);
        }
    }
}