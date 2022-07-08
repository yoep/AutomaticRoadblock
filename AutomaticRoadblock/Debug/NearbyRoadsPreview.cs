using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Utils.Road;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug
{
    public class NearbyRoadsPreview : INearbyRoadsPreview
    {
        private readonly ILogger _logger;
        private readonly IGameFiber _gameFiber;

        private List<Road> _roads;

        #region Constructors

        public NearbyRoadsPreview(ILogger logger, IGameFiber gameFiber)
        {
            _logger = logger;
            _gameFiber = gameFiber;
        }

        #endregion

        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AutomaticRoadblockPlugin.NearbyRoadsPreview);

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
            _gameFiber.NewSafeFiber(() =>
            {
                MenuItem.Text = AutomaticRoadblockPlugin.NearbyRoadsPreviewRemove;
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
            _gameFiber.NewSafeFiber(() =>
            {
                MenuItem.Text = AutomaticRoadblockPlugin.NearbyRoadsPreview;
                _roads.ForEach(x => x.DeletePreview());
                _roads = null;
            }, "RoadPreview");
        }
    }
}