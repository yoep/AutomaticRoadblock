using AutomaticRoadblocks.AbstractionLayer;
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
            const float maxLaneWidth = 25f;
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
                if (rayTracingDistance > maxLaneWidth)
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
    }
}