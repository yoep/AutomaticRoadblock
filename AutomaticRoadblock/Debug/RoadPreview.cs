using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Utils.Road;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug
{
    public class RoadPreview : IRoadPreview
    {
        private readonly ILogger _logger;
        private readonly IGameFiber _gameFiber;
        private Road _road;

        public RoadPreview(ILogger logger, IGameFiber gameFiber)
        {
            _logger = logger;
            _gameFiber = gameFiber;
        }

        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AutomaticRoadblockPlugin.RoadPreview);

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

        #region IRoadPreview

        /// <inheritdoc />
        public void ShowRoadPreview()
        {
            if (_road == null)
                CreateRoadPreview();
        }

        /// <inheritdoc />
        public void HideRoadPreview()
        {
            if (_road != null)
                RemoveRoadPreview();
        }

        #endregion

        private void CreateRoadPreview()
        {
            _gameFiber.NewSafeFiber(() =>
            {
                MenuItem.Text = AutomaticRoadblockPlugin.RoadPreviewRemove;
                _road = RoadUtils.GetClosestRoad(Game.LocalPlayer.Character.Position, RoadType.All);
                _logger.Debug("Nearest road info: " + _road);
                _road.CreatePreview();
            }, "RoadPreview");
        }

        private void RemoveRoadPreview()
        {
            _gameFiber.NewSafeFiber(() =>
            {
                MenuItem.Text = AutomaticRoadblockPlugin.RoadPreview;
                _road.DeletePreview();
                _road = null;
            }, "RoadPreview");
        }
    }
}