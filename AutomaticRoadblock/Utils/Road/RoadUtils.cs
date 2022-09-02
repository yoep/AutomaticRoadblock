using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using JetBrains.Annotations;
using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Utils.Road
{
    public static class RoadUtils
    {
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        #region Methods

        /// <summary>
        /// Get the closest road near the given position.
        /// </summary>
        /// <param name="position">Set the position to use as reference.</param>
        /// <param name="nodeType">Set the road type.</param>
        /// <returns>Returns the position of the closest road.</returns>
        public static Road FindClosestRoad(Vector3 position, EVehicleNodeType nodeType)
        {
            Road closestRoad = null;
            var closestRoadDistance = 99999f;

            foreach (var road in FindNearbyRoads(position, nodeType, 2.5f))
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
        /// <param name="blacklistedFlags">The flags of a node which should be ignored if present.</param>
        /// <returns>Returns the found round traversed from the start position.</returns>
        public static Road FindRoadTraversing(Vector3 position, float heading, float distance, EVehicleNodeType roadType, ENodeFlag blacklistedFlags)
        {
            FindVehicleNodesWhileTraversing(position, heading, distance, roadType, blacklistedFlags, out var lastFoundNode);
            return DiscoverRoadForVehicleNode(lastFoundNode);
        }

        /// <summary>
        /// Find roads while traversing the given distance from the current location.
        /// </summary>
        /// <param name="position">The position to start from.</param>
        /// <param name="heading">The heading to traverse the road from.</param>
        /// <param name="distance">The distance to traverse.</param>
        /// <param name="roadType">The road types to follow.</param>
        /// <param name="blacklistedFlags">The flags of a node which should be ignored if present.</param>
        /// <returns>Returns the roads found while traversing the distance.</returns>
        public static ICollection<Road> FindRoadsTraversing(Vector3 position, float heading, float distance, EVehicleNodeType roadType,
            ENodeFlag blacklistedFlags)
        {
            var nodeInfos = FindVehicleNodesWhileTraversing(position, heading, distance, roadType, blacklistedFlags, out _);

            var startedAt = DateTime.Now.Ticks;
            var roads = nodeInfos
                .Select(DiscoverRoadForVehicleNode)
                .ToList();
            var calculationTime = (DateTime.Now.Ticks - startedAt) / TimeSpan.TicksPerMillisecond;
            Logger.Debug($"Discovered a total of {roads.Count} roads in {calculationTime} millis while traversing the road");
            return roads;
        }

        /// <summary>
        /// Get nearby roads near the given position.
        /// </summary>
        /// <param name="position">Set the position to use as reference.</param>
        /// <param name="nodeType">The allowed node types to return.</param>
        /// <param name="radius">The radius to search for nearby roads.</param>
        /// <returns>Returns the position of the closest road.</returns>
        public static IEnumerable<Road> FindNearbyRoads(Vector3 position, EVehicleNodeType nodeType, float radius)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(nodeType, "nodeType cannot be null");
            var nodes = FindNearbyVehicleNodes(position, nodeType, radius);

            return nodes
                .Select(DiscoverRoadForVehicleNode)
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

        #endregion

        #region Functions

        [CanBeNull]
        private static Road.NodeInfo FindVehicleNodeWithHeading(Vector3 position, float heading, EVehicleNodeType nodeType,
            ENodeFlag blacklistedFlags = ENodeFlag.None)
        {
            Logger.Trace($"Searching for vehicle nodes at {position} matching heading {heading}");
            var nodes = FindNearbyVehicleNodes(position, nodeType, 2f, 20);
            var closestNodeDistance = 9999f;
            var closestNode = (Road.NodeInfo)null;

            // filter out any nodes which match one or more blacklisted conditions
            foreach (var node in nodes.Where(x => (x.Flags & blacklistedFlags) == 0))
            {
                var distance = position.DistanceTo(node.Position);

                if (distance > closestNodeDistance ||
                    Math.Abs(node.Heading - heading) % 360 > 45f)
                    continue;

                closestNodeDistance = distance;
                closestNode = node;
            }

            Logger.Debug($"Using node {closestNode} as closest matching");
            return closestNode;
        }

        private static IEnumerable<Road.NodeInfo> FindNearbyVehicleNodes(Vector3 position, EVehicleNodeType nodeType, float radius, int rotationInterval = 10)
        {
            var nodes = new List<Road.NodeInfo>();

            for (var rad = 1; rad <= radius; rad++)
            {
                for (var rot = 0; rot < 360; rot += rotationInterval)
                {
                    var x = (float)(position.X + rad * Math.Sin(rot));
                    var y = (float)(position.Y + rad * Math.Cos(rot));

                    var node = FindVehicleNode(new Vector3(x, y, position.Z + 5f), nodeType);

                    if (nodes.Contains(node))
                        continue;

                    Logger.Trace($"Discovered new vehicle node, {node}");
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        private static List<Road.NodeInfo> FindVehicleNodesWhileTraversing(Vector3 position, float heading, float distance, EVehicleNodeType nodeType,
            ENodeFlag blacklistedFlags, out Road.NodeInfo lastFoundNodeInfo)
        {
            var startedAt = DateTime.Now.Ticks;
            var nextNodeCalculationStartedAt = DateTime.Now.Ticks;
            var distanceTraversed = 0f;
            var distanceToMove = 5f;
            var findNodeAttempt = 0;
            var nodeInfos = new List<Road.NodeInfo>();

            lastFoundNodeInfo = new Road.NodeInfo(position, heading, -1, -1, -1f);

            while (distanceTraversed < distance)
            {
                if (findNodeAttempt == 10)
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
            Logger.Debug($"Traversed a total of {position.DistanceTo(lastFoundNodeInfo.Position)} distance with expectation {distance} within {calculationTime} millis\n" +
                         $"origin: {position}, destination: {lastFoundNodeInfo.Position}");
            return nodeInfos;
        }

        private static Road DiscoverRoadForVehicleNode(Road.NodeInfo nodeInfo)
        {
            var rightSideHeading = nodeInfo.Heading;
            var roadRightSide = GetLastPointOnRoad(nodeInfo.Position, rightSideHeading - 90f);
            var roadLeftSide = GetLastPointOnRoad(nodeInfo.Position, rightSideHeading + 90f);

            return new Road
            {
                RightSide = roadRightSide,
                LeftSide = roadLeftSide,
                NumberOfLanes1 = nodeInfo.NumberOfLanes1,
                NumberOfLanes2 = nodeInfo.NumberOfLanes2,
                JunctionIndicator = (int)nodeInfo.AtJunction,
                Lanes = DiscoverLanes(roadRightSide, roadLeftSide, nodeInfo.Position, rightSideHeading, nodeInfo.NumberOfLanes1,
                    nodeInfo.NumberOfLanes2),
                Node = nodeInfo,
            };
        }

        private static List<Road.Lane> DiscoverLanes(Vector3 roadRightSide, Vector3 roadLeftSide, Vector3 roadMiddle, float rightSideHeading,
            int numberOfLanes1, int numberOfLanes2)
        {
            var lanes = new List<Road.Lane>();

            // verify if there is currently only one lane
            // then calculate the lane from right to left
            if (numberOfLanes1 == 0 || numberOfLanes2 == 0)
            {
                var numberOfLanes = numberOfLanes1 == 0 ? numberOfLanes2 : numberOfLanes1;
                lanes.AddRange(CreateLanes(roadRightSide, roadLeftSide, rightSideHeading, numberOfLanes, false));
            }
            else
            {
                lanes.AddRange(CreateLanes(roadRightSide, roadMiddle, rightSideHeading, numberOfLanes1, false));
                lanes.AddRange(CreateLanes(roadLeftSide, roadMiddle, MathHelper.NormalizeHeading(rightSideHeading - 180), numberOfLanes2, true));
            }

            return lanes;
        }

        private static IEnumerable<Road.Lane> CreateLanes(Vector3 rightSidePosition, Vector3 leftSidePosition, float laneHeading, int numberOfLanes,
            bool isOpposite)
        {
            var lastRightPosition = rightSidePosition;
            var laneWidth = rightSidePosition.DistanceTo(leftSidePosition) / numberOfLanes;
            var moveDirection = MathHelper.ConvertHeadingToDirection(laneHeading + 90f);
            var lanes = new List<Road.Lane>();

            for (var index = 1; index <= numberOfLanes; index++)
            {
                var laneLeftPosition = lastRightPosition + moveDirection * laneWidth;
                var nodePosition = lastRightPosition + moveDirection * (laneWidth / 2);

                lanes.Add(new Road.Lane
                {
                    Heading = laneHeading,
                    RightSide = lastRightPosition,
                    LeftSide = laneLeftPosition,
                    NodePosition = nodePosition,
                    Width = laneWidth,
                    IsOppositeHeadingOfRoadNodeHeading = isOpposite
                });
                lastRightPosition = laneLeftPosition;
            }

            return lanes;
        }

        private static Road.NodeInfo FindVehicleNode(Vector3 position, EVehicleNodeType type)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(type, "type cannot be null");
            int nodeNumberOfLanes1;
            int nodeNumberOfLanes2;

            NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING(position.X, position.Y, position.Z, out Vector3 nodePosition, out float nodeHeading,
                (int)type, 3, 0);
            NativeFunction.Natives.GET_CLOSEST_ROAD(nodePosition.X, nodePosition.Y, nodePosition.Z, 1f, 1, out Vector3 roadPosition1, out Vector3 roadPosition2,
                out int numberOfLanes1, out int numberOfLanes2, out float junctionIndication, (int)ERoadType.All);
            NativeFunction.Natives.GET_VEHICLE_NODE_PROPERTIES<bool>(nodePosition.X, nodePosition.Y, nodePosition.Z, out int density, out int flags);

            if (roadPosition1.Equals(nodePosition))
            {
                nodeNumberOfLanes1 = numberOfLanes1;
                nodeNumberOfLanes2 = numberOfLanes2;
            }
            else if (roadPosition2.Equals(nodePosition))
            {
                nodeNumberOfLanes1 = numberOfLanes2;
                nodeNumberOfLanes2 = numberOfLanes1;
            }
            else
            {
                Logger.Warn("No road position matched the vehicle node, using default lane numbers instead");
                nodeNumberOfLanes1 = 1;
                nodeNumberOfLanes2 = 0;
            }

            return new Road.NodeInfo(nodePosition, MathHelper.NormalizeHeading(nodeHeading), nodeNumberOfLanes1, nodeNumberOfLanes2, junctionIndication)
            {
                Density = density,
                Flags = (ENodeFlag)flags
            };
        }

        private static Vector3 GetLastPointOnRoad(Vector3 position, float heading)
        {
            const float stopAtMinimumInterval = 0.2f;
            var intervalCheck = 1f;
            var direction = MathHelper.ConvertHeadingToDirection(heading);
            var roadMaterial = 0U;
            var positionToCheck = position;
            var lastCheckedPositionOnRoad = position;

            while (intervalCheck > stopAtMinimumInterval)
            {
                var positionOnTheGround = GameUtils.GetOnTheGroundPosition(positionToCheck);
                uint materialHash;

                unsafe
                {
                    bool hit;
                    Vector3 worldHitPosition;
                    Vector3 surfacePosition;
                    uint hitEntity;

                    // WORLDPROBE::_START_SHAPE_TEST_RAY
                    var handle = NativeFunction.CallByHash<int>(0x377906D8A31E5586, positionOnTheGround.X, positionOnTheGround.Y, positionOnTheGround.Z + 0.5f,
                        positionOnTheGround.X, positionOnTheGround.Y, positionOnTheGround.Z - 1f,
                        (int)ETraceFlags.IntersectWorld, Game.LocalPlayer.Character, 7);

                    // WORLDPROBE::_GET_SHAPE_TEST_RESULT_EX
                    NativeFunction.CallByHash<int>(0x65287525D951F6BE, handle, &hit, &worldHitPosition, &surfacePosition, &materialHash, &hitEntity);
                }

                if (roadMaterial == 0U)
                    roadMaterial = materialHash;

                // verify if positionOnTheGround is still on the road
                if (roadMaterial == materialHash)
                {
                    // store the last known position
                    lastCheckedPositionOnRoad = positionOnTheGround;
                }
                else
                {
                    // reduce the check distance as we're over the road
                    intervalCheck /= 2;
                }

                positionToCheck = lastCheckedPositionOnRoad + direction * intervalCheck;
            }

            return lastCheckedPositionOnRoad;
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