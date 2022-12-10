using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Menu.Switcher;
using AutomaticRoadblocks.Utils;
using Rage;
using RAGENativeUI;

namespace AutomaticRoadblocks.Roadblock.Menu
{
    public class RoadblockMenuSwitchItem : IMenuSwitchItem, IDisposable
    {
        private readonly ILocalizer _localizer;

        private bool _running;
        private bool _visible;

        public RoadblockMenuSwitchItem(ILocalizer localizer)
        {
            _localizer = localizer;

            Menu = new UIMenu(localizer[LocalizationKey.MenuTitle], 
                "~b~" + localizer[LocalizationKey.MenuSubtitle]);
        }

        /// <inheritdoc />
        public UIMenu Menu { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.Pursuit;

        /// <inheritdoc />
        public string DisplayText => _localizer[LocalizationKey.MenuPursuit];

        /// <inheritdoc />
        public void Dispose()
        {
            _running = false;
        }

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            Process();
        }

        private void Process()
        {
            _running = true;
            GameUtils.NewSafeFiber(() =>
            {
                while (_running)
                {
                    if (Menu.Visible)
                    {
                        if (!_visible)
                            SelectDispatchNowOption();

                        _visible = true;
                    }
                    else
                    {
                        _visible = false;
                    }

                    GameFiber.Yield();
                }
            }, "ManualPlacementMenuSwitchItem.Process");
        }

        private void SelectDispatchNowOption()
        {
            var item = Menu.MenuItems.First(x => x.Text.Equals(_localizer[LocalizationKey.DispatchNow]));

            // verify if the option is available before selecting it
            if (item.Enabled)
                Menu.CurrentSelection = Menu.MenuItems.IndexOf(item);
        }
    }
}