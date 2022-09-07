using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Street.Info;
using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Street.Factory
{
    internal static class IntersectionFactory
    {
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        internal static IVehicleNode Create(VehicleNodeInfo nodeInfo)
        {
            var directions = DiscoverIntersectionDirections(nodeInfo.Position);
            return new Intersection
            {
                Position = nodeInfo.Position,
                Heading = nodeInfo.Heading,
                Directions = directions,
                Roads = DiscoverConnectingRoads(directions)
            };
        }

        private static List<VehicleNodeInfo> DiscoverIntersectionDirections(Vector3 position)
        {
            var nodes = new List<VehicleNodeInfo>();

            for (var rot = 0; rot < 360; rot += 45)
            {
                var x = (float)(position.X + 1f * Math.Sin(rot));
                var y = (float)(position.Y + 1f * Math.Cos(rot));
                Vector3 nodePosition;
                float nodeHeading;

                unsafe
                {
                    // PATHFIND::GET_CLOSEST_VEHICLE_NODE_WITH_HEADING
                    NativeFunction.CallByHash<uint>(0xFF071FB798B803B0, x, y, position.Z, &nodePosition, &nodeHeading,
                        (int)EVehicleNodeType.AllNodes, 0f, 0f);
                }

                var node = new VehicleNodeInfo(nodePosition, nodeHeading);

                if (!nodes.Contains(node))
                    nodes.Add(node);
            }

            return nodes;
        }

        private static List<Road> DiscoverConnectingRoads(IReadOnlyCollection<VehicleNodeInfo> nodeInfos)
        {
            var startedAt = DateTime.Now.Ticks;
            var roads = new List<Road>();

            Logger.Trace($"Discovering intersection roads for {nodeInfos.Count} nodes");
            foreach (var node in nodeInfos)
            {
                DiscoverRoadForIntersectionNode(roads, node);
            }

            var calculationTime = (DateTime.Now.Ticks - startedAt) / TimeSpan.TicksPerMillisecond;
            Logger.Debug($"Discovered a total of {roads.Count} intersection roads in {calculationTime} millis");
            return roads;
        }

        private static void DiscoverRoadForIntersectionNode(ICollection<Road> discoveredRoads, VehicleNodeInfo directionNode)
        {
            var distance = 5f;
            var nodeAdded = false;
            var attempts = 0;
            VehicleNodeInfo lastFoundNode;

            do
            {
                if (attempts == 6)
                {
                    Logger.Warn($"Failed to discover road node for intersection road at Position: {directionNode.Position}, Heading: {directionNode.Heading}");
                    break;
                }

                var searchPosition = directionNode.Position + MathHelper.ConvertHeadingToDirection(directionNode.Heading) * distance;
                lastFoundNode = StreetHelper.FindVehicleNode(searchPosition, EVehicleNodeType.AllRoadNoJunctions, 0, 0);

                Logger.Trace($"Expecting heading: {directionNode.Heading}, found node {lastFoundNode}");
                if (directionNode.Position.Equals(lastFoundNode.Position)
                    || Math.Abs(directionNode.Heading - lastFoundNode.Heading) > 10f
                    || RoadHeadingAlreadyFound(discoveredRoads, lastFoundNode))
                {
                    distance *= 1.5f;
                    attempts++;
                }
                else
                {
                    DetermineNumberOfLanes(lastFoundNode, out var numberOfLanesSameDirection, out var numberOfLanesOppositeDirection);
                    lastFoundNode.LanesInSameDirection = numberOfLanesSameDirection;
                    lastFoundNode.LanesInOppositeDirection = numberOfLanesOppositeDirection;

                    Logger.Debug($"Using node {lastFoundNode} for intersection direction {directionNode}");
                    discoveredRoads.Add(RoadFactory.Create(lastFoundNode));
                    nodeAdded = true;
                }
            } while (!nodeAdded);
        }

        private static void DetermineNumberOfLanes(VehicleNodeInfo lastFoundNode, out int numberOfLanesSame, out int numberOfLanesOpposite)
        {
            NativeFunction.Natives.GET_CLOSEST_ROAD(lastFoundNode.Position.X, lastFoundNode.Position.Y, lastFoundNode.Position.Z, 1f, 1,
                out Vector3 roadPosition1, out Vector3 roadPosition2,
                out int numberOfLanes1, out int numberOfLanes2, out float _, (int)ERoadType.All);

            if (roadPosition1.DistanceTo(lastFoundNode.Position) < roadPosition2.DistanceTo(lastFoundNode.Position))
            {
                numberOfLanesSame = numberOfLanes1;
                numberOfLanesOpposite = numberOfLanes2;
            }
            else
            {
                numberOfLanesSame = numberOfLanes2;
                numberOfLanesOpposite = numberOfLanes1;
            }
        }

        private static bool RoadHeadingAlreadyFound(IEnumerable<Road> roads, VehicleNodeInfo node)
        {
            return roads.Any(x => Math.Abs(x.Heading - node.Heading) < 30f);
        }
    }
}