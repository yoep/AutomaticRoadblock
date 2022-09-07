using System;
using System.Collections.Generic;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Street.Info;
using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Street.Factory
{
    internal static class IntersectionFactory
    {
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        
        internal static IStreet Create(NodeInfo nodeInfo)
        {
            return new Intersection
            {
                Position = nodeInfo.Position,
                Heading = nodeInfo.Heading,
                Directions = DiscoverIntersectionDirections(nodeInfo.Position)
            };
        }

        private static List<NodeInfo> DiscoverIntersectionDirections(Vector3 position)
        {
            var nodes = new List<NodeInfo>();

            for (var rot = 0; rot < 360; rot += 45)
            {
                var x = (float)(position.X + 1f * Math.Sin(rot));
                var y = (float)(position.Y + 1f * Math.Cos(rot));
                Vector3 nodePosition;
                float nodeHeading;

                unsafe
                {
                    // PATHFIND::GET_NTH_CLOSEST_VEHICLE_NODE_ID_WITH_HEADING
                    NativeFunction.CallByHash<uint>(0xFF071FB798B803B0 , x, y, position.Z, &nodePosition, &nodeHeading,
                        (int)EVehicleNodeType.AllNodes, 0f, 0f);
                }

                var node = new NodeInfo(nodePosition, nodeHeading, 0, 0, 0f);

                if (!nodes.Contains(node))
                    nodes.Add(node);
            }

            return nodes;
        }
    }
}