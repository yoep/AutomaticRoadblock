using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Pursuit.Menu
{
    public class PursuitLevelComponent : IMenuComponent
    {
        private readonly IPursuitManager _pursuitManager;

        public PursuitLevelComponent(IPursuitManager pursuitManager)
        {
            _pursuitManager = pursuitManager;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } =
            new UIMenuListScrollerItem<int>(AutomaticRoadblocksPlugin.PursuitLevel, AutomaticRoadblocksPlugin.PursuitLevelDescription, PursuitLevel.Levels
                .Select(x => x.Level)
                .ToList());

        /// <inheritdoc />
        public MenuType Type => MenuType.PURSUIT;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            var scrollerMenuItem = (UIMenuListScrollerItem<int>)MenuItem;

            _pursuitManager.UpdatePursuitLevel(PursuitLevel.From(scrollerMenuItem.SelectedItem));
        }

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            var scrollerMenuItem = (UIMenuListScrollerItem<int>)MenuItem;
            _pursuitManager.PursuitLevelChanged += PursuitLevelChanged;
            scrollerMenuItem.IndexChanged += MenuItemChanged;
        }

        private void MenuItemChanged(UIMenuScrollerItem sender, int oldindex, int newindex)
        {
            var scrollerMenuItem = (UIMenuListScrollerItem<int>)MenuItem;
            _pursuitManager.PursuitLevel = PursuitLevel.From(scrollerMenuItem.SelectedItem);
        }

        private void PursuitLevelChanged(PursuitLevel newPursuitLevel)
        {
            var scrollerMenuItem = (UIMenuListScrollerItem<int>)MenuItem;
            scrollerMenuItem.SelectedItem = newPursuitLevel.Level;
        }
    }
}