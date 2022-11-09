using System.Collections.Generic;
using AutomaticRoadblocks.Street.Info;
using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Street
{
    internal static class StreetHelper
    {
        internal static IEnumerable<VehicleNodeInfo> FindNearbyVehicleNodes(Vector3 position, EVehicleNodeType nodeType)
        {
            NativeFunction.Natives.GET_CLOSEST_ROAD(position.X, position.Y, position.Z, 1f, 1, out Vector3 roadPosition1, out Vector3 roadPosition2,
                out int numberOfLanes1, out int numberOfLanes2, out float _, (int)ERoadType.All);

            return new List<VehicleNodeInfo>
            {
                FindVehicleNode(roadPosition1, nodeType, numberOfLanes1, numberOfLanes2),
                FindVehicleNode(roadPosition2, nodeType, numberOfLanes2, numberOfLanes1)
            };
        }

        internal static VehicleNodeInfo FindVehicleNode(Vector3 position, EVehicleNodeType type)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(type, "type cannot be null");
            NativeFunction.Natives.GET_CLOSEST_ROAD(position.X, position.Y, position.Z, 1f, 1, out Vector3 roadPosition1, out Vector3 roadPosition2,
                out int numberOfLanes1, out int numberOfLanes2, out float _, (int)ERoadType.All);

            return position.DistanceTo2D(roadPosition1) < position.DistanceTo2D(roadPosition2)
                ? FindVehicleNode(roadPosition1, type, numberOfLanes1, numberOfLanes2)
                : FindVehicleNode(roadPosition1, type, numberOfLanes2, numberOfLanes1);
        }

        internal static VehicleNodeInfo FindVehicleNode(Vector3 position, EVehicleNodeType type, int lanesSameDirection, int lanesOppositeDirection)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(type, "type cannot be null");

            NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING(position.X, position.Y, position.Z, out Vector3 nodePosition, out float nodeHeading,
                (int)type, 3, 0);
            NativeFunction.Natives.GET_VEHICLE_NODE_PROPERTIES<bool>(nodePosition.X, nodePosition.Y, nodePosition.Z, out int density, out int flags);

            return new VehicleNodeInfo(nodePosition, MathHelper.NormalizeHeading(nodeHeading))
            {
                LanesInSameDirection = lanesSameDirection,
                LanesInOppositeDirection = lanesOppositeDirection,
                Density = density,
                Flags = (ENodeFlag)flags
            };
        }
    }
}