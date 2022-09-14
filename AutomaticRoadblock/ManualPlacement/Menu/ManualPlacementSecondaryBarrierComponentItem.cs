using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Models;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualPlacementSecondaryBarrierComponentItem : AbstractBarrierComponentItem
    {
        public ManualPlacementSecondaryBarrierComponentItem(IManualPlacement manualPlacement, IModelProvider modelProvider, ILocalizer localizer)
            : base(manualPlacement, modelProvider, localizer, LocalizationKey.SecondaryBarrier, LocalizationKey.SecondaryBarrierDescription)
        {
        }

        /// <inheritdoc />
        protected override void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            ManualPlacement.SecondaryBarrier = MenuItem.SelectedItem;
        }

        protected override void SelectInitialMenuItem()
        {
            MenuItem.SelectedItem = ManualPlacement.SecondaryBarrier;
        }
    }
}