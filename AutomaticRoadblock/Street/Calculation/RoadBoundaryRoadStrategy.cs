using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Street.Calculation
{
    public class RoadBoundaryRoadStrategy : IRoadStrategy
    {
        public bool Calculate(Vector3 position, float heading, out Vector3 lastPointOnRoad)
        {
            Vector3 lastPositionNative;
            bool result;

            unsafe
            {
                // PATHFIND::GET_ROAD_BOUNDARY_USING_HEADING
                result = NativeFunction.CallByHash<bool>(0xA0F8A7517A273C05, position.X, position.Y, position.Z, heading, &lastPositionNative);
                lastPointOnRoad = lastPositionNative;
            }

            return result;
        }
    }
}