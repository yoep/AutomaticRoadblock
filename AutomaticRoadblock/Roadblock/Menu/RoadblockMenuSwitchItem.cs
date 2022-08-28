using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Menu.Switcher;
using RAGENativeUI;

namespace AutomaticRoadblocks.Roadblock.Menu
{
    public class RoadblockMenuSwitchItem : IMenuSwitchItem, IDisposable
    {
        private readonly IGame _game;

        private bool _running;
        private bool _visible;

        public RoadblockMenuSwitchItem(IGame game)
        {
            _game = game;
        }

        /// <inheritdoc />
        public UIMenu Menu { get; } = new(AutomaticRoadblocksPlugin.MenuTitle, AutomaticRoadblocksPlugin.MenuSubtitle);

        /// <inheritdoc />
        public MenuType Type => MenuType.Pursuit;

        /// <inheritdoc />
        public string DisplayText => AutomaticRoadblocksPlugin.MenuPursuit;

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
            _game.NewSafeFiber(() =>
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

                    _game.FiberYield();
                }
            }, "ManualPlacementMenuSwitchItem.Process");
        }

        private void SelectDispatchNowOption()
        {
            var item = Menu.MenuItems.First(x => x.Text.Equals(AutomaticRoadblocksPlugin.DispatchNow));

            // verify if the option is available before selecting it
            if (item.Enabled)
                Menu.CurrentSelection = Menu.MenuItems.IndexOf(item);
        }
    }
}