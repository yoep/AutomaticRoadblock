using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Menu.Switcher;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Utils.Type;
using Rage;
using RAGENativeUI;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualPlacementMenuSwitchItem : IMenuSwitchItem, IDisposable
    {
        private readonly IGame _game;

        private bool _running = true;

        public ManualPlacementMenuSwitchItem(IGame game)
        {
            _game = game;
        }

        /// <inheritdoc />
        public UIMenu Menu { get; } = new(AutomaticRoadblocksPlugin.MenuTitle, AutomaticRoadblocksPlugin.MenuSubtitle);

        /// <inheritdoc />
        public MenuType Type => MenuType.MANUAL_PLACEMENT;

        /// <inheritdoc />
        public string DisplayText => AutomaticRoadblocksPlugin.MenuManualPlacement;

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
            _game.NewSafeFiber(() =>
            {
                while (_running)
                {
                    _game.FiberYield();

                    if (Menu.Visible)
                    {
                        var position = _game.PlayerPosition;
                        var renderDirection = MathHelper.ConvertHeadingToDirection(_game.PlayerHeading);
                        var road = RoadUtils.FindClosestRoad(position + renderDirection * 5f, RoadType.All);

                        GameUtils.CreateMarker(road.Position, MarkerType.MarkerTypeVerticalCylinder, Color.LightBlue, 2.5f, false);
                    }
                }
            }, "ManualPlacementMenuSwitchItem.Process");
        }
    }
}