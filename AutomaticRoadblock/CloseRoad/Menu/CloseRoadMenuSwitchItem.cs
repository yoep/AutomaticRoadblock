using System;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Menu.Switcher;
using RAGENativeUI;

namespace AutomaticRoadblocks.CloseRoad.Menu
{
    public class CloseRoadMenuSwitchItem : IMenuSwitchItem, IDisposable
    {
        private readonly ILocalizer _localizer;

        public CloseRoadMenuSwitchItem(ILocalizer localizer)
        {
            _localizer = localizer;

            Menu = new UIMenu(_localizer[LocalizationKey.MenuTitle],
                "~b~" + _localizer[LocalizationKey.MenuSubtitle]);
        }

        /// <inheritdoc />
        public UIMenu Menu { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.CloseRoad;

        /// <inheritdoc />
        public string DisplayText => _localizer[LocalizationKey.MenuCloseRoad];

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}