using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Models;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualPlacementMainBarrierComponentItem : AbstractBarrierComponentItem
    {
        public ManualPlacementMainBarrierComponentItem(IManualPlacement manualPlacement, IModelProvider modelProvider, ILocalizer localizer)
            : base(manualPlacement, modelProvider, localizer, LocalizationKey.MainBarrier, LocalizationKey.MainBarrierDescription)
        {
        }

        /// <inheritdoc />
        protected override void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            ManualPlacement.MainBarrier = MenuItem.SelectedItem;
        }

        protected override void SelectInitialMenuItem()
        {
            MenuItem.SelectedItem = ManualPlacement.MainBarrier;
        }
    }
}