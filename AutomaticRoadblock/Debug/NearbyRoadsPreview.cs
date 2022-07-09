using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Utils.Road;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug
{
    public class NearbyRoadsPreview : IMenuComponent
    {
        private readonly ILogger _logger;
        private readonly IGame _game;

        private List<Road> _roads;

        #region Constructors

        public NearbyRoadsPreview(ILogger logger, IGame game)
        {
            _logger = logger;
            _game = game;
        }

        #endregion

        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AutomaticRoadblocksPlugin.NearbyRoadsPreview);

        /// <inheritdoc />
        public MenuType Type => MenuType.DEBUG;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            if (_roads != null)
            {
                RemoveRoadsPreview();
            }
            else
            {
                CreateRoadsPreview();
            }
        }

        #endregion

        private void CreateRoadsPreview()
        {
            _game.NewSafeFiber(() =>
            {
                MenuItem.Text = AutomaticRoadblocksPlugin.NearbyRoadsPreviewRemove;
                _roads = RoadUtils.GetNearbyRoads(Game.LocalPlayer.Character.Position, RoadType.All).ToList();
                _logger.Debug("--- NEARBY ROADS ---");
                _roads.ForEach(x =>
                {
                    _logger.Debug(x.ToString());
                    x.CreatePreview();
                });
            }, "RoadPreview");
        }

        private void RemoveRoadsPreview()
        {
            _game.NewSafeFiber(() =>
            {
                MenuItem.Text = AutomaticRoadblocksPlugin.NearbyRoadsPreview;
                _roads.ForEach(x => x.DeletePreview());
                _roads = null;
            }, "RoadPreview");
        }
    }
}