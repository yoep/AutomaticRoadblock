using System.Collections.Generic;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Street
{
    internal static class StreetHelper
    {
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        internal static bool LastPointOnRoadUsingNative(Vector3 position, float heading, out Vector3 lastPointPosition)
        {
            Vector3 lastPositionNative;
            bool result;

            unsafe
            {
                // PATHFIND::GET_ROAD_BOUNDARY_USING_HEADING
                result = NativeFunction.CallByHash<bool>(0xA0F8A7517A273C05, position.X, position.Y, position.Z, heading, &lastPositionNative);
                lastPointPosition = lastPositionNative;
            }

            return result;
        }

        internal static bool LastPointOnRoadUsingRaytracing(Vector3 position, float heading, out Vector3 lastPointOnRoad)
        {
            const float stopAtMinimumInterval = 0.2f;
            const float maxRoadWidth = 35f;
            var intervalCheck = 2f;
            var direction = MathHelper.ConvertHeadingToDirection(heading);
            var roadMaterial = 0U;
            var positionToCheck = position;
            lastPointOnRoad = position;

            Logger.Trace($"Calculating last point on road for position: {position}, heading: {heading}");
            while (intervalCheck > stopAtMinimumInterval)
            {
                var rayTracingDistance = position.DistanceTo(lastPointOnRoad);

                // verify if we're not exceeding the maximum
                // as this indicates that the raytracing isn't working as it supposed to
                if (rayTracingDistance > maxRoadWidth)
                {
                    Logger.Warn(
                        $"Failed to calculate the last point on road using raytracing for position: {position}, heading: {heading} (tracing distance: {rayTracingDistance})");
                    return false;
                }

                var positionOnTheGround = GameUtils.GetOnTheGroundPosition(positionToCheck);
                uint materialHash;

                unsafe
                {
                    bool hit;
                    Vector3 worldHitPosition;
                    Vector3 surfacePosition;
                    uint hitEntity;

                    // SHAPETEST::START_EXPENSIVE_SYNCHRONOUS_SHAPE_TEST_LOS_PROBE
                    var handle = NativeFunction.CallByHash<int>(0x377906D8A31E5586, positionOnTheGround.X, positionOnTheGround.Y, positionOnTheGround.Z + 1f,
                        positionOnTheGround.X, positionOnTheGround.Y, positionOnTheGround.Z - 1f,
                        (int)ETraceFlags.IntersectWorld, Game.LocalPlayer.Character, 7);

                    // WORLDPROBE::GET_SHAPE_TEST_RESULT_INCLUDING_MATERIAL
                    NativeFunction.CallByHash<int>(0x65287525D951F6BE, handle, &hit, &worldHitPosition, &surfacePosition, &materialHash, &hitEntity);
                }

                if (roadMaterial == 0U)
                    roadMaterial = materialHash;

                // verify if positionOnTheGround is still on the road
                if (roadMaterial == materialHash)
                {
                    // store the last known position
                    lastPointOnRoad = positionOnTheGround;
                }
                else
                {
                    // reduce the check distance as we're over the road
                    intervalCheck /= 2;
                }

                positionToCheck = lastPointOnRoad + direction * intervalCheck;
            }

            return true;
        }
        
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