using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Street.Factory;
using AutomaticRoadblocks.Street.Info;
using JetBrains.Annotations;
using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Street
{
    public static class RoadQuery
    {
        private const float NodeHeadingTolerance = 55f;
        
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        #region Methods

        /// <summary>
        /// Find the closest vehicle node to the given position.
        /// </summary>
        /// <param name="position">Set the position to use as reference.</param>
        /// <param name="nodeType">Set the road type.</param>
        /// <returns></returns>
        public static VehicleNodeInfo FindClosestNode(Vector3 position, EVehicleNodeType nodeType)
        {
            VehicleNodeInfo closestVehicleNode = null;
            var closestRoadDistance = 99999f;

            foreach (var road in StreetHelper.FindNearbyVehicleNodes(position, nodeType))
            {
                var roadDistanceToPosition = Vector3.Distance2D(road.Position, position);

                if (roadDistanceToPosition > closestRoadDistance)
                    continue;

                closestVehicleNode = road;
                closestRoadDistance = roadDistanceToPosition;
            }

            return closestVehicleNode;
        }

        /// <summary>
        /// Get the closest road near the given position.
        /// </summary>
        /// <param name="position">Set the position to use as reference.</param>
        /// <param name="nodeType">Set the road type.</param>
        /// <returns>Returns the position of the closest road.</returns>
        public static IVehicleNode FindClosestRoad(Vector3 position, EVehicleNodeType nodeType)
        {
            return ToVehicleNode(FindClosestNode(position, nodeType));
        }

        /// <summary>
        /// Find a road by traversing the current road position towards the heading.
        /// </summary>
        /// <param name="position">The position to start from.</param>
        /// <param name="heading">The heading to traverse the road from.</param>
        /// <param name="distance">The distance to traverse.</param>
        /// <param name="roadType">The road types to follow.</param>
        /// <param name="blacklistedFlags">The flags of a node which should be ignored if present.</param>
        /// <returns>Returns the found round traversed from the start position.</returns>
        public static IVehicleNode FindRoadTraversing(Vector3 position, float heading, float distance, EVehicleNodeType roadType, ENodeFlag blacklistedFlags)
        {
            FindVehicleNodesWhileTraversing(position, heading, distance, roadType, blacklistedFlags, out var lastFoundNode);
            var startedAt = DateTime.Now.Ticks;
            var road = ToVehicleNode(lastFoundNode);
            var calculationTime = (DateTime.Now.Ticks - startedAt) / TimeSpan.TicksPerMillisecond;
            Logger.Debug($"Converted the vehicle node into a road in {calculationTime} millis");
            return road;
        }

        /// <summary>
        /// Find streets while traversing the given distance from the current location.
        /// This collects each discovered node while it traverses the expected distance.
        /// </summary>
        /// <param name="position">The position to start from.</param>
        /// <param name="heading">The heading to traverse the road from.</param>
        /// <param name="distance">The distance to traverse.</param>
        /// <param name="roadType">The road types to follow.</param>
        /// <param name="blacklistedFlags">The flags of a node which should be ignored if present.</param>
        /// <returns>Returns the roads found while traversing the distance.</returns>
        /// <remarks>The last element in the collection will always be of type <see cref="EStreetType.Road"/>.</remarks>
        public static ICollection<IVehicleNode> FindRoadsTraversing(Vector3 position, float heading, float distance, EVehicleNodeType roadType,
            ENodeFlag blacklistedFlags)
        {
            var nodeInfos = FindVehicleNodesWhileTraversing(position, heading, distance, roadType, blacklistedFlags, out _);
            var startedAt = DateTime.Now.Ticks;
            var roads = nodeInfos
                .Select(ToVehicleNode)
                .ToList();
            var calculationTime = (DateTime.Now.Ticks - startedAt) / TimeSpan.TicksPerMillisecond;
            Logger.Debug($"Converted a total of {nodeInfos.Count} nodes to roads in {calculationTime} millis");
            return roads;
        }

        /// <summary>
        /// Get nearby roads near the given position.
        /// </summary>
        /// <param name="position">Set the position to use as reference.</param>
        /// <param name="nodeType">The allowed node types to return.</param>
        /// <param name="radius">The radius to search for nearby roads.</param>
        /// <returns>Returns the position of the closest road.</returns>
        public static IEnumerable<IVehicleNode> FindNearbyRoads(Vector3 position, EVehicleNodeType nodeType, float radius)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(nodeType, "nodeType cannot be null");
            var nodes = FindNearbyVehicleNodes(position, nodeType, radius);

            return nodes
                .Select(ToVehicleNode)
                .ToList();
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
            //VEHICLE::_ADD_SPEED_ZONE_FOR_COORD
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
        /// Verify if the given position is a dirt/offroad location based on the slow road flag.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>Returns true when the position is a dirt/offroad, else false.</returns>
        public static bool IsSlowRoad(Vector3 position)
        {
            var nodeId = GetClosestNodeId(position);
            return IsSlowRoad(nodeId);
        }

        /// <summary>
        /// Convert the given node info into actual road information.
        /// </summary>
        /// <param name="nodeInfo">The vehicle node info to convert.</param>
        /// <returns>Returns the discovered road info for the given node.</returns>
        public static IVehicleNode ToVehicleNode(VehicleNodeInfo nodeInfo)
        {
            return nodeInfo.Flags.HasFlag(ENodeFlag.IsJunction)
                ? IntersectionFactory.Create(nodeInfo)
                : RoadFactory.Create(nodeInfo);
        }

        #endregion

        #region Functions

        [CanBeNull]
        private static VehicleNodeInfo FindVehicleNodeWithHeading(Vector3 position, float heading, EVehicleNodeType nodeType,
            ENodeFlag blacklistedFlags = ENodeFlag.None)
        {
            Logger.Trace($"Searching for vehicle nodes at {position} matching heading {heading} and type {nodeType}");
            var nodes = StreetHelper.FindNearbyVehicleNodes(position, nodeType).ToList();
            var closestNodeDistance = 9999f;
            var closestNode = (VehicleNodeInfo)null;
            var ignoreHeading = false;

            // filter out any nodes which match one or more blacklisted conditions
            var filteredNodes = nodes
                .Where(x => (x.Flags & blacklistedFlags) == 0)
                .Where(x => Math.Abs(x.Heading - heading) <= NodeHeadingTolerance)
                .ToList();

            if (filteredNodes.Count == 0)
            {
                ignoreHeading = true;
                filteredNodes = nodes.Where(x => (x.Flags & blacklistedFlags) == 0).ToList();
            }

            foreach (var node in filteredNodes)
            {
                var distance = position.DistanceTo(node.Position);

                if (distance > closestNodeDistance)
                    continue;

                closestNodeDistance = distance;
                closestNode = node;
            }

            if (closestNode != null && ignoreHeading)
            {
                closestNode = new VehicleNodeInfo(closestNode.Position, closestNode.Heading)
                {
                    LanesInSameDirection = closestNode.LanesInSameDirection,
                    LanesInOppositeDirection = closestNode.LanesInOppositeDirection,
                    Density = closestNode.Density,
                    Flags = closestNode.Flags
                };
            }

            Logger.Debug(closestNode != null
                ? $"Using node {closestNode} as closest matching"
                : $"No matching node found for position: {position} with heading {heading}");

            return closestNode;
        }

        private static List<VehicleNodeInfo> FindVehicleNodesWhileTraversing(Vector3 position, float heading, float expectedDistance, EVehicleNodeType nodeType,
            ENodeFlag blacklistedFlags, out VehicleNodeInfo lastFoundNodeInfo)
        {
            var startedAt = DateTime.Now.Ticks;
            var nextNodeCalculationStartedAt = DateTime.Now.Ticks;
            var distanceTraversed = 0f;
            var distanceToMove = 5f;
            var findNodeAttempt = 0;
            var nodeInfos = new List<VehicleNodeInfo>();

            lastFoundNodeInfo = new VehicleNodeInfo(position, heading);

            // keep traversing the road while the expected distance isn't reached
            // or we've ended at a junction (never end at a junction as it actually never can be used)
            while (distanceTraversed < expectedDistance || lastFoundNodeInfo.Flags.HasFlag(ENodeFlag.IsJunction))
            {
                if (findNodeAttempt == 5)
                {
                    Logger.Warn($"Failed to traverse road, unable to find next node after {lastFoundNodeInfo} (tried {findNodeAttempt} times)");
                    break;
                }

                var findNodeAt = lastFoundNodeInfo.Position + MathHelper.ConvertHeadingToDirection(lastFoundNodeInfo.Heading) * distanceToMove;
                var nodeTowardsHeading = FindVehicleNodeWithHeading(findNodeAt, lastFoundNodeInfo.Heading, nodeType, blacklistedFlags);

                if (nodeTowardsHeading == null
                    || nodeTowardsHeading.Position.Equals(lastFoundNodeInfo.Position)
                    || nodeInfos.Any(x => x.Equals(nodeTowardsHeading)))
                {
                    distanceToMove *= 1.5f;
                    findNodeAttempt++;
                }
                else
                {
                    var additionalDistanceTraversed = lastFoundNodeInfo.Position.DistanceTo(nodeTowardsHeading.Position);
                    var nextNodeCalculationDuration = (DateTime.Now.Ticks - nextNodeCalculationStartedAt) / TimeSpan.TicksPerMillisecond;
                    nodeInfos.Add(nodeTowardsHeading);
                    distanceToMove = 5f;
                    findNodeAttempt = 0;
                    distanceTraversed += additionalDistanceTraversed;
                    lastFoundNodeInfo = nodeTowardsHeading;
                    Logger.Trace(
                        $"Traversed an additional {additionalDistanceTraversed} distance in {nextNodeCalculationDuration} millis, new total traversed distance = {distanceTraversed}");
                    nextNodeCalculationStartedAt = DateTime.Now.Ticks;
                }
            }

            var calculationTime = (DateTime.Now.Ticks - startedAt) / TimeSpan.TicksPerMillisecond;
            Logger.Debug(
                $"Traversed a total of {position.DistanceTo(lastFoundNodeInfo.Position)} distance with expectation {expectedDistance} within {calculationTime} millis\n" +
                $"origin: {position}, destination: {lastFoundNodeInfo.Position}");
            return nodeInfos;
        }

        private static IEnumerable<VehicleNodeInfo> FindNearbyVehicleNodes(Vector3 position, EVehicleNodeType nodeType, float radius,
            int rotationInterval = 20)
        {
            const int radInterval = 2;
            var nodes = new List<VehicleNodeInfo>();
            var rad = 1;

            while (rad <= radius)
            {
                for (var rot = 0; rot < 360; rot += rotationInterval)
                {
                    var x = (float)(position.X + rad * Math.Sin(rot));
                    var y = (float)(position.Y + rad * Math.Cos(rot));

                    var node = StreetHelper.FindVehicleNode(new Vector3(x, y, position.Z + 5f), nodeType);

                    if (nodes.Contains(node))
                        continue;

                    Logger.Trace($"Discovered new vehicle node, {node}");
                    nodes.Add(node);
                }

                if (radius % radInterval != 0 && rad == (int)radius - (int)radius % radInterval)
                {
                    rad += (int)radius % radInterval;
                }
                else
                {
                    rad += radInterval;
                }
            }

            return nodes;
        }

        private static int GetClosestNodeId(Vector3 position)
        {
            return NativeFunction.CallByName<int>("GET_NTH_CLOSEST_VEHICLE_NODE_ID", position.X, position.Y, position.Z, 1, 1, 1077936128, 0f);
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
}