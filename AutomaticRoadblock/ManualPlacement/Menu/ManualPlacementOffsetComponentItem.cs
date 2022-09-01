using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualPlacementOffsetComponentItem : IMenuComponent<UIMenuNumericScrollerItem<double>>
    {
        private readonly IManualPlacement _manualPlacement;

        public ManualPlacementOffsetComponentItem(IManualPlacement manualPlacement, ILocalizer localizer)
        {
            _manualPlacement = manualPlacement;

            MenuItem = new UIMenuNumericScrollerItem<double>(
                localizer[LocalizationKey.Offset], localizer[LocalizationKey.OffsetDescription], -10f, 10f, 0.1f);
        }

        /// <inheritdoc />
        public UIMenuNumericScrollerItem<double> MenuItem { get; }

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
            MenuItem.Value = _manualPlacement.Offset;
            MenuItem.IndexChanged += ValueChanged;
        }

        private void ValueChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _manualPlacement.Offset = (float)MenuItem.Value;
        }
    }
}