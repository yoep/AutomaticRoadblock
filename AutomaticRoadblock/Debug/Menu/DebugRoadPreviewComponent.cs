using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Street;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugRoadPreviewComponent : IMenuComponent<UIMenuListItem>
    {
        private const float SearchRadius = 15f;

        private readonly ILogger _logger;
        private readonly IGame _game;
        private List<IVehicleNode> _roads;

        public DebugRoadPreviewComponent(ILogger logger, IGame game)
        {
            _logger = logger;
            _game = game;
        }

        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuListItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.RoadPreview, AutomaticRoadblocksPlugin.RoadPreviewDescription,
            new DisplayItem(ERoadPreviewType.Closest, AutomaticRoadblocksPlugin.RoadPreviewClosest),
            new DisplayItem(ERoadPreviewType.ClosestNoJunction, AutomaticRoadblocksPlugin.RoadPreviewClosestNoJunction),
            new DisplayItem(ERoadPreviewType.NearbyAll, AutomaticRoadblocksPlugin.RoadPreviewNearby),
            new DisplayItem(ERoadPreviewType.NearbyNoJunction, AutomaticRoadblocksPlugin.RoadPreviewNearbyNoJunction)
        );

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
                var playerPosition = Game.LocalPlayer.Character.Position;

                _roads = type switch
                {
                    ERoadPreviewType.Closest => new List<IVehicleNode>
                    {
                        RoadQuery.FindClosestRoad(playerPosition, EVehicleNodeType.AllNodes)
                    },
                    ERoadPreviewType.ClosestNoJunction => new List<IVehicleNode>
                    {
                        RoadQuery.FindClosestRoad(playerPosition, EVehicleNodeType.AllRoadNoJunctions)
                    },
                    ERoadPreviewType.NearbyAll => RoadQuery
                        .FindNearbyRoads(playerPosition, EVehicleNodeType.AllNodes, SearchRadius)
                        .ToList(),
                    ERoadPreviewType.NearbyNoJunction => RoadQuery
                        .FindNearbyRoads(playerPosition, EVehicleNodeType.AllRoadNoJunctions, SearchRadius)
                        .ToList(),
                    _ => _roads
                };

                _logger.Debug("Nearest road info: " + string.Join("---\n", _roads));
                _roads.ForEach(x => x.CreatePreview());
            }, "RoadPreviewComponent.Preview");
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
            ClosestNoJunction,
            NearbyAll,
            NearbyNoJunction
        }
    }
}