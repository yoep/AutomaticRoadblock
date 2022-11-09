using Rage;

namespace AutomaticRoadblocks.Street.Calculation
{
    public interface IRoadStrategy
    {
        bool Calculate(Vector3 position, float heading, out Vector3 lastPointOnRoad);
    }
}