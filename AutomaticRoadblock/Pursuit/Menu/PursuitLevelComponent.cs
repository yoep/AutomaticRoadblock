using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Pursuit.Menu
{
    public class PursuitLevelComponent : IMenuComponent<UIMenuListScrollerItem<int>>
    {
        private readonly IPursuitManager _pursuitManager;

        public PursuitLevelComponent(IPursuitManager pursuitManager, ILocalizer localizer)
        {
            _pursuitManager = pursuitManager;

            MenuItem =
                new UIMenuListScrollerItem<int>(localizer[LocalizationKey.PursuitLevel], localizer[LocalizationKey.PursuitLevelDescription], PursuitLevel.Levels
                    .Select(x => x.Level)
                    .ToList());
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<int> MenuItem { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.Pursuit;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _pursuitManager.PursuitLevel = PursuitLevel.From(MenuItem.SelectedItem);
        }

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            _pursuitManager.PursuitLevelChanged += PursuitLevelChanged;
            MenuItem.SelectedItem = _pursuitManager.PursuitLevel.Level;
            MenuItem.IndexChanged += MenuItemChanged;
        }

        private void MenuItemChanged(UIMenuScrollerItem sender, int oldindex, int newindex)
        {
            _pursuitManager.PursuitLevel = PursuitLevel.From(MenuItem.SelectedItem);
        }

        private void PursuitLevelChanged(PursuitLevel newPursuitLevel)
        {
            MenuItem.SelectedItem = newPursuitLevel.Level;
        }
    }
}