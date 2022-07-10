using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Utils.Road;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug
{
    public class RoadPreviewComponent : IMenuComponent
    {
        private readonly ILogger _logger;
        private readonly IGame _game;
        private Road _road;

        public RoadPreviewComponent(ILogger logger, IGame game)
        {
            _logger = logger;
            _game = game;
        }

        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AutomaticRoadblocksPlugin.RoadPreview);

        /// <inheritdoc />
        public MenuType Type => MenuType.DEBUG;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            if (_road == null)
            {
                CreateRoadPreview();
            }
            else
            {
                RemoveRoadPreview();
            }
        }

        #endregion

        private void CreateRoadPreview()
        {
            _game.NewSafeFiber(() =>
            {
                MenuItem.Text = AutomaticRoadblocksPlugin.RoadPreviewRemove;
                _road = RoadUtils.GetClosestRoad(Game.LocalPlayer.Character.Position, RoadType.All);
                _logger.Debug("Nearest road info: " + _road);
                _road.CreatePreview();
            }, "RoadPreview");
        }

        private void RemoveRoadPreview()
        {
            _game.NewSafeFiber(() =>
            {
                MenuItem.Text = AutomaticRoadblocksPlugin.RoadPreview;
                _road.DeletePreview();
                _road = null;
            }, "RoadPreview");
        }
    }
}