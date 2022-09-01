using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Utils.Road;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
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
            new DisplayItem(ERoadPreviewType.Closest, AutomaticRoadblocksPlugin.RoadPreviewClosest),
            new DisplayItem(ERoadPreviewType.Nearby, AutomaticRoadblocksPlugin.RoadPreviewNearby));

        /// <inheritdoc />
        public EMenuType Type => EMenuType.Debug;

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
                var type = (ERoadPreviewType)MenuItem.SelectedValue;
                _roads = type == ERoadPreviewType.Closest
                    ? new List<Road> { RoadUtils.FindClosestRoad(Game.LocalPlayer.Character.Position, ERoadType.All) }
                    : RoadUtils.FindNearbyRoads(Game.LocalPlayer.Character.Position, ERoadType.All).ToList();

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

        public enum ERoadPreviewType
        {
            Closest,
            Nearby
        }
    }
}