using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.ManualPlacement;
using AutomaticRoadblocks.Street;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.CloseRoad
{
    public class RoadClosureInstance : IRoadClosure
    {
        private const float IntersectionRoadblockOffset = 10f;
        private readonly IVehicleNode _mainNode;

        private readonly ILogger _logger = IoC.Instance.GetInstance<ILogger>();
        private readonly List<IVehicleNode> _nodes = new();
        private readonly List<ManualRoadblock> _roadblocks = new();
        private readonly List<ARCloseNodes> _closeNodes = new();

        public RoadClosureInstance(IVehicleNode mainNode, EBackupUnit backupUnit, BarrierModel barrier, LightModel lightSource, float maxDistance)
        {
            Assert.NotNull(mainNode, "mainNode cannot be null");
            _mainNode = mainNode;
            BackupUnit = backupUnit;
            Barrier = barrier;
            LightSource = lightSource;
            MaxDistance = maxDistance;

            Init();
        }

        #region Properties

        /// <summary>
        /// The backup unit type to use.
        /// </summary>
        private EBackupUnit BackupUnit { get; }

        /// <summary>
        /// The barrier to use when closing the road.
        /// </summary>
        private BarrierModel Barrier { get; }

        /// <summary>
        /// The light source to use when closing the road.
        /// </summary>
        private LightModel LightSource { get; }

        /// <summary>
        /// The max distance from this main node for which the road should be closed.
        /// </summary>
        private float MaxDistance { get; }

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive { get; internal set; }

        /// <inheritdoc />
        public void CreatePreview()
        {
            if (IsPreviewActive)
                return;

            IsPreviewActive = true;
            _mainNode.CreatePreview();
            _nodes.ForEach(x => x.CreatePreview());
            _roadblocks.ForEach(x => x.CreatePreview());
            _closeNodes.ForEach(x => x.CreatePreview());
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            if (!IsPreviewActive)
                return;

            _mainNode.DeletePreview();
            _nodes.ForEach(x => x.DeletePreview());
            _roadblocks.ForEach(x => x.DeletePreview());
            _closeNodes.ForEach(x => x.DeletePreview());
            IsPreviewActive = false;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Spawn()
        {
            _roadblocks.ForEach(x => x.Spawn());
            _closeNodes.ForEach(x => x.Spawn());
            _logger.Info($"Close road spawned a total of {_roadblocks.Count} roadblocks and {_closeNodes.Count} node closures");
        }

        /// <inheritdoc />
        public void Release()
        {
            _roadblocks.ForEach(x => x.Release(true));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                _mainNode.DeletePreview();
                _nodes.ForEach(x => x.DeletePreview());
                _nodes.Clear();
                _roadblocks.ForEach(SafeDisposeRoadblock);
                _roadblocks.Clear();
                _closeNodes.ForEach(x => x.Dispose());
                _closeNodes.Clear();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to dispose close road instance, {ex.Message}", ex);
            }
        }

        #endregion

        #region Functions

        private void Init()
        {
            if (_mainNode.Type == EStreetType.Intersection)
            {
                CloseIntersection((Intersection)_mainNode);
            }
            else
            {
                CloseRoad((Road)_mainNode);
            }
        }

        private void CloseIntersection(Intersection node)
        {
            _nodes.Add(node);
            _logger.Debug($"Closing a total of {node.Roads.Count} roads for intersection");
            foreach (var road in node.Roads)
            {
                var roadblockTopPosition = road.RightSide + MathHelper.ConvertHeadingToDirection(road.Heading) * IntersectionRoadblockOffset;
                _roadblocks.Add(
                    new ManualRoadblock(CreateRequest(road, road.Heading - 180, PlacementType.OppositeDirectionOfRoad, -IntersectionRoadblockOffset)));
                _closeNodes.Add(new ARCloseNodes(roadblockTopPosition, node.Position));
            }
        }

        private void CloseRoad(Road road)
        {
            var isGravelRoad = RoadQuery.IsSlowRoad(road.Position);
            var nodeType = isGravelRoad ? EVehicleNodeType.AllNodes : EVehicleNodeType.MainRoadsWithJunctions;
            var blacklistedNodes = isGravelRoad
                ? ENodeFlag.IsOnWater | ENodeFlag.IsAlley
                : ENodeFlag.IsAlley | ENodeFlag.IsGravelRoad | ENodeFlag.IsBackroad | ENodeFlag.IsOnWater;
            var laneClosestToPlayer = road.LaneClosestTo(Game.LocalPlayer.Character.Position);
            var directionClosure = road.IsSingleDirection ? "one" : "two";

            _logger.Info($"Road closure will close {directionClosure} way at {road.Position}");
            CloseRoadForHeading(road, MathHelper.NormalizeHeading(laneClosestToPlayer.Heading - 180), nodeType, blacklistedNodes);

            // verify if the other side also needs to be closed
            if (!road.IsSingleDirection)
            {
                CloseRoadForHeading(road, MathHelper.NormalizeHeading(laneClosestToPlayer.Heading), nodeType, blacklistedNodes);
            }
        }

        private void CloseRoadForHeading(Road road, float heading, EVehicleNodeType nodeType, ENodeFlag blacklistedNodes)
        {
            _logger.Trace($"Searching node to close for {road.Position} heading {heading}");
            var nodeToClose = FindPositionToCloseFor(road.Position, heading, nodeType, blacklistedNodes);
            var targetHeading = MathHelper.NormalizeHeading(nodeToClose.LanesSameDirection.First().Heading - 180);
            var nodeClosureVehicleNodesPosition = nodeToClose.LeftSide + MathHelper.ConvertHeadingToDirection(nodeToClose.Heading) * 3f;

            _logger.Debug($"Closing road for node {nodeToClose} with targetHeading {targetHeading}");
            _roadblocks.Add(new ManualRoadblock(CreateRequest(nodeToClose, targetHeading, PlacementType.OppositeDirectionOfRoad, 0f)));
            _closeNodes.Add(new ARCloseNodes(road.Position, nodeClosureVehicleNodesPosition));
        }

        private ManualRoadblock.Request CreateRequest(Road node, float heading, PlacementType placementType, float offset)
        {
            return new ManualRoadblock.Request
            {
                Road = node,
                MainBarrier = Barrier,
                SecondaryBarrier = BarrierModel.None,
                BackupType = BackupUnit,
                PlacementType = placementType,
                TargetHeading = heading,
                CopsEnabled = true,
                Offset = offset,
                AddLights = LightSource != LightModel.None && GameUtils.TimePeriod is ETimePeriod.Evening or ETimePeriod.Night,
                LightSources = new List<LightModel> { LightSource }
            };
        }

        private Road FindPositionToCloseFor(Vector3 position, float heading, EVehicleNodeType nodeType, ENodeFlag blacklistedNodes)
        {
            var nodes = RoadQuery.FindRoadsTraversing(position, heading, MaxDistance, nodeType, blacklistedNodes, true);
            _nodes.AddRange(nodes);
            _logger.Trace($"Found nodes as potential road closure, [{string.Join(", ", _nodes)}]");

            try
            {
                _logger.Debug($"Filtering out road closure position out of {_nodes.Count}");
                return nodes
                    .Where(x => x.Type == EStreetType.Road)
                    .Select(x => (Road)x)
                    .Last();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to find position from road traversal, {ex.Message}", ex);
            }

            return FindRoadFromIntersectionFallback(nodes.Last(), heading);
        }

        private void SafeDisposeRoadblock(ManualRoadblock roadblock)
        {
            try
            {
                roadblock.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to dispose roadblock, {ex.Message}", ex);
            }
        }

        private Road FindRoadFromIntersectionFallback(IVehicleNode node, float wantedHeading)
        {
            _logger.Warn($"Using intersection road as fallback at {node.Position}");
            var closestHeadingDiff = 9999f;
            var bestMatchingNode = (Road)null;

            foreach (var road in ((Intersection)node).Roads)
            {
                var headingDiff = MathHelper.NormalizeHeading(wantedHeading - road.Heading);

                if (headingDiff > closestHeadingDiff)
                    continue;

                closestHeadingDiff = headingDiff;
                bestMatchingNode = road;
            }

            _logger.Debug($"Using best matching road {bestMatchingNode}");
            return bestMatchingNode;
        }

        #endregion
    }
}