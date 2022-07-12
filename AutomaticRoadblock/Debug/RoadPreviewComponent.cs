using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Utils.Road;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug
{
    public class RoadPreviewComponent : IMenuComponent<UIMenuListItem>
    {
        private readonly ILogger _logger;
        private readonly IGame _game;
        private List<Road> _roads;

        public RoadPreviewComponent(ILogger logger, IGame game)
        {
            _logger = logger;
            _game = game;
        }

        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuListItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.RoadPreview, AutomaticRoadblocksPlugin.RoadPreviewDescription,
            new DisplayItem(RoadPreviewType.Closest, AutomaticRoadblocksPlugin.RoadPreviewClosest),
            new DisplayItem(RoadPreviewType.Nearby, AutomaticRoadblocksPlugin.RoadPreviewNearby));

        /// <inheritdoc />
        public MenuType Type => MenuType.DEBUG;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            if (_roads == null)
            {
                CreateRoadPreview();
            }
            else
            {
                RemoveRoadPreview();
            }
        }

        #endregion

        [Conditional("DEBUG")]
        private void CreateRoadPreview()
        {
            _game.NewSafeFiber(() =>
            {
                MenuItem.Text = AutomaticRoadblocksPlugin.RoadPreviewRemove;
                var type = (RoadPreviewType)MenuItem.SelectedValue;
                _roads = type == RoadPreviewType.Closest
                    ? new List<Road> { RoadUtils.GetClosestRoad(Game.LocalPlayer.Character.Position, RoadType.All) }
                    : RoadUtils.GetNearbyRoads(Game.LocalPlayer.Character.Position, RoadType.All).ToList();

                _logger.Debug("Nearest road info: " + string.Join("---\n", _roads));
                _roads.ForEach(x => x.CreatePreview());
            }, "RoadPreview");
        }

        [Conditional("DEBUG")]
        private void RemoveRoadPreview()
        {
            _game.NewSafeFiber(() =>
            {
                MenuItem.Text = AutomaticRoadblocksPlugin.RoadPreview;
                _roads.ForEach(x => x.DeletePreview());
                _roads = null;
            }, "RoadPreview");
        }

        public enum RoadPreviewType
        {
            Closest,
            Nearby
        }
    }
}