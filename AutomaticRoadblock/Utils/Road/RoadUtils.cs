using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Utils.Road
{
    public static class RoadUtils
    {
        private const float MaxLaneWidth = 10f;

        #region Methods

        /// <summary>
        /// Get the closest road near the given position.
        /// </summary>
        /// <param name="position">Set the position to use as reference.</param>
        /// <param name="roadType">Set the road type.</param>
        /// <returns>Returns the position of the closest road.</returns>
        public static Road FindClosestRoad(Vector3 position, RoadType roadType)
        {
            Road closestRoad = null;
            var closestRoadDistance = 99999f;

            foreach (var road in FindNearbyRoads(position, roadType))
            {
                var roadDistanceToPosition = Vector3.Distance2D(road.Position, position);

                if (roadDistanceToPosition > closestRoadDistance)
                    continue;

                closestRoad = road;
                closestRoadDistance = roadDistanceToPosition;
            }

            return closestRoad;
        }

        /// <summary>
        /// Find a road by traversing the current road position towards the heading.
        /// </summary>
        /// <param name="position">The position to start from.</param>
        /// <param name="heading">The heading to traverse the road from.</param>
        /// <param name="distance">The distance to traverse.</param>
        /// <param name="roadType">The road types to follow.</param>
        /// <returns>Returns the found round traversed from the start position.</returns>
        public static Road FindRoadTraversing(Vector3 position, float heading, float distance, VehicleNodeType roadType)
        {
            FindVehicleNodesWhileTraversing(position, heading, distance, roadType, out var lastFoundNode);
            return DiscoverRoad(lastFoundNode);
        }

        /// <summary>
        /// Find roads while traversing the given distance from the current location.
        /// </summary>
        /// <param name="position">The position to start from.</param>
        /// <param name="heading">The heading to traverse the road from.</param>
        /// <param name="distance">The distance to traverse.</param>
        /// <param name="roadType">The road types to follow.</param>
        /// <returns>Returns the roads found while traversing the distance.</returns>
        public static ICollection<Road> FindRoadsTraversing(Vector3 position, float heading, float distance, VehicleNodeType roadType)
        {
            var nodeInfos = FindVehicleNodesWhileTraversing(position, heading, distance, roadType, out _);

            return nodeInfos
                .Select(DiscoverRoad)
                .ToList();
        }

        /// <summary>
        /// Get nearby roads near the given position.
        /// </summary>
        /// <param name="position">Set the position to use as reference.</param>
        /// <param name="roadType">Set the road type.</param>
        /// <returns>Returns the position of the closest road.</returns>
        public static IEnumerable<Road> FindNearbyRoads(Vector3 position, RoadType roadType)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(roadType, "roadType cannot be null");
            var vehicleNodeType = Convert(roadType);

            FindVehicleNodes(position, vehicleNodeType, out NodeInfo node1, out NodeInfo node2);

            return new List<Road>
            {
                DiscoverRoad(node1),
                DiscoverRoad(node2)
            };
        }

        /// <summary>
        /// Get the opposite heading of the given heading.
        /// </summary>
        /// <param name="heading">Set the heading.</param>
        /// <returns>Returns the opposite heading of the given heading.</returns>
        public static float OppositeHeading(float heading)
        {
            return (heading + 180) % 360;
        }

        /// <summary>
        /// Check if the given lane is the left side lane of the road.
        /// </summary>
        /// <param name="road">The road to determine the lane side of.</param>
        /// <param name="lane">The lane on the road to verify.</param>
        /// <returns>Returns true if the given lane is the left lane of the road, else false.</returns>
        public static bool IsLeftSideLane(Road road, Road.Lane lane)
        {
            var distanceRightSide = Vector3.Distance2D(road.RightSide, lane.Position);
            var distanceLeftSide = Vector3.Distance2D(road.LeftSide, lane.Position);

            return distanceLeftSide < distanceRightSide;
        }

        /// <summary>
        /// Check if the road has multiple lanes in the same direction as the given lane.
        /// </summary>
        /// <param name="road">The road to verify the lanes of.</param>
        /// <param name="lane">The lane to check for same direction.</param>
        /// <returns>Returns true if the road has multiple lanes going in the same direction as the given lane.</returns>
        public static bool HasMultipleLanesInSameDirection(Road road, Road.Lane lane)
        {
            Assert.NotNull(road, "road cannot be null");
            return road.Lanes
                .Where(x => x != lane)
                .Any(x => Math.Abs(lane.Heading - x.Heading) < 1f);
        }

        /// <summary>
        /// Create a new speed zone.
        /// </summary>
        /// <param name="position">The position of the zone.</param>
        /// <param name="radius">The radius of the zone.</param>
        /// <param name="speed">The max speed within the zone.</param>
        /// <returns>Returns the created zone ID.</returns>
        public static int CreateSpeedZone(Vector3 position, float radius, float speed)
        {
            Assert.NotNull(position, "position cannot be null");
            return NativeFunction.CallByHash<int>(0x2CE544C68FB812A0, position.X, position.Y, position.Z, radius, speed, false);
        }

        /// <summary>
        /// Remove a speed zone.
        /// </summary>
        /// <param name="zoneId">The zone id to remove.</param>
        public static void RemoveSpeedZone(int zoneId)
        {
            Assert.NotNull(zoneId, "zoneId cannot be null");
            NativeFunction.CallByHash<int>(0x2CE544C68FB812A0, zoneId);
        }

        /// <summary>
        /// Verify if the given point is on a road.
        /// </summary>
        /// <param name="position">The point position to check.</param>
        /// <returns>Returns true if the position is on a road, else false.</returns>
        public static bool IsPointOnRoad(Vector3 position)
        {
            return NativeFunction.Natives.IS_POINT_ON_ROAD<bool>(position.X, position.Y, position.Z);
        }

        #endregion

        #region Functions

        private static VehicleNodeType Convert(RoadType roadType)
        {
            return roadType switch
            {
                RoadType.MajorRoads => VehicleNodeType.MainRoads,
                RoadType.MajorRoadsNoJunction => VehicleNodeType.AllRoadNoJunctions,
                _ => VehicleNodeType.AllNodes
            };
        }

        private static NodeInfo FindVehicleNodeWithHeading(Vector3 position, float heading, VehicleNodeType roadType)
        {
            var logger = IoC.Instance.GetInstance<ILogger>();
            logger.Trace($"Searching for vehicle nodes at {position} matching heading {heading}");
            FindVehicleNodes(position, roadType, out var node1, out var node2);
            logger.Trace($"Found vehicle node 1 at {node1.Position} with heading {node1.Heading}");
            logger.Trace($"Found vehicle node 2 at {node2.Position} with heading {node2.Heading}");

            var nodeMatchingClosestToHeading = Math.Abs(node1.Heading - heading) % 360 < Math.Abs(node2.Heading - heading) % 360 ? node1 : node2;
            logger.Debug($"Using node {nodeMatchingClosestToHeading} as closest matching");

            if (Math.Abs(nodeMatchingClosestToHeading.Heading - heading) % 360 > 45)
            {
                logger.Warn(
                    $"Closest matching node {nodeMatchingClosestToHeading} is exceeding the heading tolerance, using the original heading of {heading} instead");
                nodeMatchingClosestToHeading = new NodeInfo(nodeMatchingClosestToHeading.Position, heading, nodeMatchingClosestToHeading.NumberOfLanes1,
                    nodeMatchingClosestToHeading.NumberOfLanes2, nodeMatchingClosestToHeading.AtJunction);
            }

            return nodeMatchingClosestToHeading;
        }

        private static List<NodeInfo> FindVehicleNodesWhileTraversing(Vector3 position, float heading, float distance, VehicleNodeType roadType,
            out NodeInfo lastFoundNode)
        {
            var distanceTraversed = 0f;
            var distanceToMove = 5f;
            var nodeInfos = new List<NodeInfo>();

            lastFoundNode = new NodeInfo(position, heading, -1, -1, -1f);

            while (distanceTraversed < distance)
            {
                var findNodeAt = lastFoundNode.Position + MathHelper.ConvertHeadingToDirection(lastFoundNode.Heading) * distanceToMove;
                var nodeTowardsHeading = FindVehicleNodeWithHeading(findNodeAt, lastFoundNode.Heading, roadType);

                if (nodeTowardsHeading.Position == lastFoundNode.Position)
                {
                    distanceToMove *= 1.5f;
                }
                else
                {
                    nodeInfos.Add(nodeTowardsHeading);
                    distanceToMove = 5f;
                    distanceTraversed += lastFoundNode.Position.DistanceTo(nodeTowardsHeading.Position);
                    lastFoundNode = nodeTowardsHeading;
                }
            }

            return nodeInfos;
        }

        private static void FindVehicleNodes(Vector3 position, VehicleNodeType roadType, out NodeInfo node1, out NodeInfo node2)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(roadType, "roadType cannot be null");
            NativeFunction.Natives.GET_CLOSEST_ROAD(position.X, position.Y, position.Z, 1f, 1, out Vector3 roadPosition1, out Vector3 roadPosition2,
                out int numberOfLanes1, out int numberOfLanes2, out float junctionIndication, (int)roadType);

            var vehicleNode1 = FindVehicleNode(roadPosition1, roadType);
            var vehicleNode2 = FindVehicleNode(roadPosition2, roadType);

            node1 = new NodeInfo(vehicleNode1.Position, vehicleNode1.Heading, numberOfLanes1, numberOfLanes2, junctionIndication);
            node2 = new NodeInfo(vehicleNode2.Position, vehicleNode2.Heading, numberOfLanes2, numberOfLanes1, junctionIndication);
        }

        private static Road DiscoverRoad(NodeInfo nodeInfo)
        {
            var rightSideHeading = nodeInfo.Heading;
            var roadRightSide = GetLastPointOnTheLane(nodeInfo.Position, rightSideHeading - 90f, nodeInfo.NumberOfLanes1);
            var roadLeftSide = GetLastPointOnTheLane(nodeInfo.Position, rightSideHeading + 90f, nodeInfo.NumberOfLanes2);

            // Fix a side if it's the same as the middle of the road as GetLastPointOnTheLane probably failed to determine the last point
            if (roadRightSide == nodeInfo.Position)
                roadRightSide = FixFailedLastPointCalculation(nodeInfo.Position, roadLeftSide, rightSideHeading - 90f);
            if (roadLeftSide == nodeInfo.Position)
                roadLeftSide = FixFailedLastPointCalculation(nodeInfo.Position, roadRightSide, rightSideHeading + 90f);

            return RoadBuilder.Builder()
                .Position(nodeInfo.Position)
                .RightSide(roadRightSide)
                .LeftSide(roadLeftSide)
                .NumberOfLanes1(nodeInfo.NumberOfLanes1)
                .NumberOfLanes2(nodeInfo.NumberOfLanes2)
                .JunctionIndicator((int)nodeInfo.AtJunction)
                .Lanes(DiscoverLanes(roadRightSide, roadLeftSide, nodeInfo.Position, rightSideHeading, nodeInfo.NumberOfLanes1, nodeInfo.NumberOfLanes2))
                .Node(new Road.VehicleNode
                {
                    Position = nodeInfo.Position,
                    Heading = nodeInfo.Heading
                })
                .Build();
        }

        private static List<Road.Lane> DiscoverLanes(Vector3 roadRightSide, Vector3 roadLeftSide, Vector3 roadMiddle, float rightSideHeading,
            int numberOfLanes1, int numberOfLanes2)
        {
            var singleDirection = IsSingleDirectionRoad(numberOfLanes1, numberOfLanes2);
            var lanes = new List<Road.Lane>();

            if (singleDirection)
            {
                var numberOfLanes = numberOfLanes1 == 0 ? numberOfLanes2 : numberOfLanes1;
                var laneWidth = GetWidth(roadRightSide, roadLeftSide) / numberOfLanes;
                lanes.AddRange(CreateLanes(roadRightSide, rightSideHeading, numberOfLanes, laneWidth, false));
            }
            else
            {
                var laneWidthRight = GetWidth(roadRightSide, roadMiddle) / numberOfLanes1;
                var laneWidthLeft = GetWidth(roadMiddle, roadLeftSide) / numberOfLanes2;

                lanes.AddRange(CreateLanes(roadRightSide, rightSideHeading, numberOfLanes1, laneWidthRight, false));
                lanes.AddRange(CreateLanes(roadMiddle, rightSideHeading, numberOfLanes2, laneWidthLeft, true));
            }

            return lanes;
        }

        private static IEnumerable<Road.Lane> CreateLanes(Vector3 roadRightSide, float rightSideHeading, int numberOfLanes, float laneWidth,
            bool isOpposite)
        {
            var lastRightPosition = roadRightSide;
            var moveDirection = MathHelper.ConvertHeadingToDirection(rightSideHeading + 90f);
            var lanes = new List<Road.Lane>();

            for (var index = 1; index <= numberOfLanes; index++)
            {
                var laneLeftPosition = lastRightPosition + moveDirection * laneWidth;
                var vehicleNode = FindVehicleNode(lastRightPosition, VehicleNodeType.AllNodes);
                var heading = isOpposite ? OppositeHeading(rightSideHeading) : rightSideHeading;
                var type = DetermineLaneType(index, numberOfLanes);

                lanes.Add(LaneBuilder.Builder()
                    .Number(index)
                    .Heading(heading)
                    .RightSide(isOpposite ? laneLeftPosition : lastRightPosition)
                    .LeftSide(isOpposite ? lastRightPosition : laneLeftPosition)
                    .NodePosition(vehicleNode.Position)
                    .Width(laneWidth)
                    .Type(type)
                    .Build());
                lastRightPosition = laneLeftPosition;
            }

            return lanes;
        }

        private static Road.Lane.LaneType DetermineLaneType(int index, int numberOfLanes)
        {
            if (index == numberOfLanes)
            {
                return Road.Lane.LaneType.RightLane;
            }

            return index == 1 ? Road.Lane.LaneType.LeftLane : Road.Lane.LaneType.MiddleLane;
        }

        // This fix is a simple workaround if the last point detection failed with the native function of GTA

        // It will assume that both sides have the same width and mirror the width to the other side to determine the point

        private static Vector3 FixFailedLastPointCalculation(Vector3 roadPosition, Vector3 otherSidePosition, float heading)
        {
            var widthOtherSide = Vector3.Distance2D(roadPosition, otherSidePosition);
            var directionOfTheSideToFix = MathHelper.ConvertHeadingToDirection(heading);
            return roadPosition + directionOfTheSideToFix * widthOtherSide;
        }

        private static NodeInfo FindVehicleNode(Vector3 position, VehicleNodeType type)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(type, "type cannot be null");
            NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING(position.X, position.Y, position.Z, out Vector3 nodePosition, out float nodeHeading,
                (int)type, 3, 0);

            return new NodeInfo(nodePosition, MathHelper.NormalizeHeading(nodeHeading));
        }

        private static Vector3 GetLastPointOnTheLane(Vector3 position, float heading, int numberOfLanes)
        {
            var checkInterval = 0.5f;
            var direction = MathHelper.ConvertHeadingToDirection(heading);
            var lastPositionOnTheRoad = position;
            var maxWidth = numberOfLanes != 0 ? numberOfLanes * MaxLaneWidth : 2 * MaxLaneWidth;

            do
            {
                var pointToCheck = lastPositionOnTheRoad + direction * checkInterval;
                if (IsPointOnRoad(pointToCheck))
                {
                    checkInterval *= 2f;
                    lastPositionOnTheRoad = pointToCheck;
                }
                else
                {
                    checkInterval /= 1.5f;
                }
            } while (checkInterval >= 0.25f && position.DistanceTo(lastPositionOnTheRoad) < maxWidth);

            return lastPositionOnTheRoad;
        }

        private static float GetWidth(Vector3 point1, Vector3 point2)
        {
            return Vector3.Distance(point1, point2);
        }

        private static bool IsSingleDirectionRoad(int numberOfLanes1, int numberOfLanes2)
        {
            return numberOfLanes1 == 0 || numberOfLanes2 == 0;
        }

        #endregion
    }

    internal class RoadBuilder
    {
        private readonly List<Road.Lane> _lanes = new();
        private Vector3 _position;
        private Vector3 _rightSide;
        private Vector3 _leftSide;
        private int _numberOfLanes1;
        private int _numberOfLanes2;
        private int _junctionIndicator;
        private Road.VehicleNode _node;

        private RoadBuilder()
        {
        }

        public static RoadBuilder Builder()
        {
            return new RoadBuilder();
        }

        public RoadBuilder Position(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _position = position;
            return this;
        }

        public RoadBuilder RightSide(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _rightSide = position;
            return this;
        }

        public RoadBuilder LeftSide(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _leftSide = position;
            return this;
        }

        public RoadBuilder NumberOfLanes1(int value)
        {
            _numberOfLanes1 = value;
            return this;
        }

        public RoadBuilder NumberOfLanes2(int value)
        {
            _numberOfLanes2 = value;
            return this;
        }

        public RoadBuilder JunctionIndicator(int value)
        {
            _junctionIndicator = value;
            return this;
        }

        public RoadBuilder Lanes(List<Road.Lane> lanes)
        {
            Assert.NotNull(lanes, "lanes cannot be null");
            _lanes.AddRange(lanes);
            return this;
        }

        public RoadBuilder Node(Road.VehicleNode node)
        {
            Assert.NotNull(node, "node cannot be null");
            _node = node;
            return this;
        }

        public Road Build()
        {
            Assert.NotNull(_position, "position has not been set");
            Assert.NotNull(_rightSide, "rightSide has not been set");
            Assert.NotNull(_leftSide, "leftSide has not been set");
            return new Road(_position, _rightSide, _leftSide, _lanes.AsReadOnly(), _node, _numberOfLanes1, _numberOfLanes2, _junctionIndicator);
        }
    }

    internal class LaneBuilder
    {
        private int _number;
        private float _heading;
        private Vector3 _rightSide;
        private Vector3 _leftSide;
        private Vector3 _nodePosition;
        private float _width;
        private Road.Lane.LaneType _type;

        private LaneBuilder()
        {
        }

        public static LaneBuilder Builder()
        {
            return new LaneBuilder();
        }

        public LaneBuilder Number(int number)
        {
            _number = number;
            return this;
        }

        public LaneBuilder Heading(float heading)
        {
            Assert.NotNull(heading, "heading cannot be null");
            _heading = heading;
            return this;
        }

        public LaneBuilder RightSide(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _rightSide = position;
            return this;
        }

        public LaneBuilder LeftSide(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _leftSide = position;
            return this;
        }

        public LaneBuilder NodePosition(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _nodePosition = position;
            return this;
        }

        public LaneBuilder Width(float width)
        {
            Assert.NotNull(width, "width cannot be null");
            _width = width;
            return this;
        }

        public LaneBuilder Type(Road.Lane.LaneType type)
        {
            Assert.NotNull(type, "type cannot be null");
            _type = type;
            return this;
        }

        public Road.Lane Build()
        {
            return new Road.Lane(_number, _heading, _rightSide, _leftSide, _nodePosition, _width, _type);
        }
    }

    internal class NodeInfo
    {
        public NodeInfo(Vector3 position, float heading)
        {
            Position = position;
            Heading = heading;
        }

        public NodeInfo(Vector3 position, float heading, int numberOfLanes1, int numberOfLanes2, float atJunction)
        {
            Position = position;
            Heading = heading;
            NumberOfLanes1 = numberOfLanes1;
            NumberOfLanes2 = numberOfLanes2;
            AtJunction = atJunction;
        }

        public Vector3 Position { get; }

        public float Heading { get; }

        public int NumberOfLanes1 { get; }

        public int NumberOfLanes2 { get; }

        public float AtJunction { get; }

        public override string ToString()
        {
            return $"{nameof(Position)}: {Position}, {nameof(Heading)}: {Heading}";
        }
    }
}