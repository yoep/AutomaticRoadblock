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

        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

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

            FindVehicleNodes(position, vehicleNodeType, roadType, out var node1, out var node2);

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

        /// <summary>
        /// Verify if the given position is a dirt/offroad location.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>Returns true when the position is a dirt/offroad, else false.</returns>
        public static bool IsDirtOrOffroad(Vector3 position)
        {
            var nodeId = GetClosestNodeId(position);
            return IsSlowRoad(nodeId);
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

        private static RoadType Convert(VehicleNodeType nodeType)
        {
            return nodeType switch
            {
                VehicleNodeType.MainRoads => RoadType.MajorRoadsNoJunction,
                VehicleNodeType.MainRoadsWithJunctions => RoadType.MajorRoads,
                _ => RoadType.All
            };
        }

        private static NodeInfo FindVehicleNodeWithHeading(Vector3 position, float heading, VehicleNodeType nodeType, RoadType roadType)
        {
            Logger.Trace($"Searching for vehicle nodes at {position} matching heading {heading}");
            FindVehicleNodes(position, nodeType, roadType, out var node1, out var node2);
            Logger.Trace($"Found vehicles nodes: {node1.Position} with heading {node1.Heading}, {node2.Position} with heading {node2.Heading}");

            var nodeMatchingClosestToHeading = Math.Abs(node1.Heading - heading) % 360 < Math.Abs(node2.Heading - heading) % 360 ? node1 : node2;
            Logger.Debug($"Using node {nodeMatchingClosestToHeading} as closest matching");

            if (Math.Abs(nodeMatchingClosestToHeading.Heading - heading) % 360 > 45)
            {
                Logger.Warn(
                    $"Closest matching node {nodeMatchingClosestToHeading} is exceeding the heading tolerance, using the original heading of {heading} instead");
                nodeMatchingClosestToHeading = new NodeInfo(nodeMatchingClosestToHeading.Position, heading, nodeMatchingClosestToHeading.NumberOfLanes1,
                    nodeMatchingClosestToHeading.NumberOfLanes2, nodeMatchingClosestToHeading.AtJunction);
            }

            return nodeMatchingClosestToHeading;
        }

        private static List<NodeInfo> FindVehicleNodesWhileTraversing(Vector3 position, float heading, float distance, VehicleNodeType nodeType,
            out NodeInfo lastFoundNode)
        {
            var startedAt = DateTime.Now.Ticks;
            var distanceTraversed = 0f;
            var distanceToMove = 5f;
            var findNodeAttempt = 0;
            var roadType = Convert(nodeType);
            var nodeInfos = new List<NodeInfo>();

            lastFoundNode = new NodeInfo(position, heading, -1, -1, -1f);

            while (distanceTraversed < distance)
            {
                if (findNodeAttempt == 10)
                {
                    Logger.Warn($"Failed to traverse road, unable to find next node after {lastFoundNode} (tried {findNodeAttempt} times)");
                    break;
                }

                var findNodeAt = lastFoundNode.Position + MathHelper.ConvertHeadingToDirection(lastFoundNode.Heading) * distanceToMove;
                var nodeTowardsHeading = FindVehicleNodeWithHeading(findNodeAt, lastFoundNode.Heading, nodeType, roadType);

                if (nodeTowardsHeading.Position.Equals(lastFoundNode.Position) || nodeInfos.Any(x => x.Equals(nodeTowardsHeading)))
                {
                    distanceToMove *= 1.5f;
                    findNodeAttempt++;
                }
                else
                {
                    var additionalDistanceTraversed = lastFoundNode.Position.DistanceTo(nodeTowardsHeading.Position);
                    nodeInfos.Add(nodeTowardsHeading);
                    distanceToMove = 5f;
                    findNodeAttempt = 0;
                    distanceTraversed += additionalDistanceTraversed;
                    lastFoundNode = nodeTowardsHeading;
                    Logger.Trace($"Traversed an additional {additionalDistanceTraversed} distance, new total traversed distance = {distanceTraversed}");
                }
            }

            var calculationTime = (DateTime.Now.Ticks - startedAt) / TimeSpan.TicksPerMillisecond;
            Logger.Debug(
                $"Traversed a total of {position.DistanceTo(lastFoundNode.Position)} distance with expectation {distance} within {calculationTime} millis\n" +
                $"origin: {position}, destination: {lastFoundNode.Position}");
            return nodeInfos;
        }

        private static void FindVehicleNodes(Vector3 position, VehicleNodeType nodeType, RoadType roadType, out NodeInfo node1, out NodeInfo node2)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(nodeType, "roadType cannot be null");
            NativeFunction.Natives.GET_CLOSEST_ROAD(position.X, position.Y, position.Z, 1f, 1, out Vector3 roadPosition1, out Vector3 roadPosition2,
                out int numberOfLanes1, out int numberOfLanes2, out float junctionIndication, (int)roadType);
            Logger.Trace(
                $"Found road with Position 1: {roadPosition1}, Position 2: {roadPosition2}, numberOfLanes1: {numberOfLanes1}, numberOfLanes2: {numberOfLanes2}, junctionIndication: {junctionIndication}\n" +
                $"Based on Position: {position}, {nameof(RoadType)}: {roadType}");

            var vehicleNode1 = FindVehicleNode(roadPosition1, nodeType);
            var vehicleNode2 = FindVehicleNode(roadPosition2, nodeType);

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

            return new Road
            {
                Position = nodeInfo.Position,
                RightSide = roadRightSide,
                LeftSide = roadLeftSide,
                NumberOfLanes1 = nodeInfo.NumberOfLanes1,
                NumberOfLanes2 = nodeInfo.NumberOfLanes2,
                JunctionIndicator = (int)nodeInfo.AtJunction,
                Lanes = DiscoverLanes(roadRightSide, roadLeftSide, nodeInfo.Position, rightSideHeading, nodeInfo.NumberOfLanes1, nodeInfo.NumberOfLanes2),
                Node = new Road.VehicleNode
                {
                    Position = nodeInfo.Position,
                    Heading = nodeInfo.Heading
                },
            };
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

                lanes.Add(new Road.Lane
                {
                    Number = index,
                    Heading = heading,
                    RightSide = isOpposite ? laneLeftPosition : lastRightPosition,
                    LeftSide = isOpposite ? lastRightPosition : laneLeftPosition,
                    NodePosition = vehicleNode.Position,
                    Width = laneWidth,
                    IsOppositeDirectionOfRoad = isOpposite
                });
                lastRightPosition = laneLeftPosition;
            }

            return lanes;
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
                1, 3, 0);

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

        private static int GetClosestNodeId(Vector3 position)
        {
            return NativeFunction.CallByName<int>("GET_NTH_CLOSEST_VEHICLE_NODE_ID", position.X, position.Y, position.Z, 1, 1, 1077936128, 0f);
        }

        public static NodeProperties GetVehicleNodeProperties(Vector3 position)
        {
            NativeFunction.Natives.GET_VEHICLE_NODE_PROPERTIES<bool>(position.X, position.Y, position.Z, out int density, out int flags);

            return new NodeProperties
            {
                Density = density,
                Flags = flags
            };
        }

        /// <summary>
        /// Verify if the given node is is offroad.
        /// </summary>
        /// <param name="nodeId">The node id to check.</param>
        /// <returns>Returns true when the node is an alley, dirt road or carpark.</returns>
        private static bool IsSlowRoad(int nodeId)
        {
            // PATHFIND::_GET_IS_SLOW_ROAD_FLAG
            return NativeFunction.CallByHash<bool>(0x4F5070AA58F69279, nodeId);
        }

        #endregion
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

        protected bool Equals(NodeInfo other)
        {
            return Position.Equals(other.Position) && Heading.Equals(other.Heading) && NumberOfLanes1 == other.NumberOfLanes1 &&
                   NumberOfLanes2 == other.NumberOfLanes2 && AtJunction.Equals(other.AtJunction);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NodeInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Heading.GetHashCode();
                hashCode = (hashCode * 397) ^ NumberOfLanes1;
                hashCode = (hashCode * 397) ^ NumberOfLanes2;
                hashCode = (hashCode * 397) ^ AtJunction.GetHashCode();
                return hashCode;
            }
        }
    }

    public class NodeProperties
    {
        public int Density { get; internal set; }

        public int Flags { get; internal set; }
    }
}