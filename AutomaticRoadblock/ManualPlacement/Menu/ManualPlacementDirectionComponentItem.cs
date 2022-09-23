using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualPlacementDirectionComponentItem : IMenuComponent<UIMenuListScrollerItem<PlacementDirection>>
    {
        private readonly IManualPlacement _manualPlacement;
        private readonly ILocalizer _localizer;

        public ManualPlacementDirectionComponentItem(IManualPlacement manualPlacement, ILocalizer localizer)
        {
            _manualPlacement = manualPlacement;
            _localizer = localizer;

            MenuItem = new UIMenuListScrollerItem<PlacementDirection>(localizer[LocalizationKey.Direction], localizer[LocalizationKey.DirectionDescription],
                new[]
                {
                    PlacementDirection.Towards,
                    PlacementDirection.Away
                });
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<PlacementDirection> MenuItem { get; }

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
            MenuItem.Formatter = direction => direction == PlacementDirection.Towards
                ? _localizer[LocalizationKey.DirectionTowardsPlayer]
                : _localizer[LocalizationKey.DirectionAwayFromPlayer];
            MenuItem.SelectedItem = _manualPlacement.Direction;
            MenuItem.IndexChanged += MenuIndexChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _manualPlacement.Direction = MenuItem.SelectedItem;
        }
    }
}