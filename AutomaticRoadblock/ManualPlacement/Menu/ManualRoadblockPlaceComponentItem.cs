using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualRoadblockPlaceComponentItem : IMenuComponent<UIMenuItem>
    {
        private readonly IManualPlacement _manualPlacement;

        public ManualRoadblockPlaceComponentItem(IManualPlacement manualPlacement, ILocalizer localizer)
        {
            _manualPlacement = manualPlacement;

            MenuItem = new UIMenuItem(localizer[LocalizationKey.Place], localizer[LocalizationKey.PlaceDescription]);
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.ManualPlacement;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _manualPlacement.PlaceRoadblock();
        }
    }
}