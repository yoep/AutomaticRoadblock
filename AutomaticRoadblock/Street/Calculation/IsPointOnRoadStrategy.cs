using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Street.Calculation
{
    public class IsPointOnRoadStrategy : IRoadStrategy
    {
        private const float MaxAttempts = 20;

        public bool Calculate(Vector3 position, float heading, out Vector3 lastPointOnRoad)
        {
            var positionToCheck = position;
            var direction = MathHelper.ConvertHeadingToDirection(heading) * 1f;
            var attempts = 0;
            bool result;

            do
            {
                result = NativeFunction.Natives.IS_POINT_ON_ROAD<bool>(positionToCheck.X, positionToCheck.Y, positionToCheck.Z, null);
                if (result)
                {
                    positionToCheck += direction;
                }
                
                attempts++;
            } while (result && attempts < MaxAttempts);

            lastPointOnRoad = positionToCheck;
            return true;
        }
    }
}