using System.Collections.Generic;
using AutomaticRoadblocks.Logging;
using AutomaticRoadblocks.Street.Calculation;
using Rage;

namespace AutomaticRoadblocks.Street.Factory
{
    public static class LastPointOnRoadStrategy
    {
        private const float FallbackRoadWidth = 5f;

        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        private static readonly List<IRoadStrategy> Strategies = new()
        {
            new RayTracingRoadStrategy(),
            new RoadBoundaryRoadStrategy(),
            new IsPointOnRoadStrategy()
        };

        public static Vector3 CalculateLastPointOnRoad(Vector3 position, float heading)
        {
            foreach (var strategy in Strategies)
            {
                Logger.Trace($"Using strategy {strategy} to calculate last point on road for position {position}");
                if (strategy.Calculate(position, heading, out position))
                {
                    Logger.Debug($"Found position {position} with strategy {strategy}");
                    return position;
                }
            }

            Logger.Warn($"Using default fallback of {FallbackRoadWidth} for calculating last point on road");
            return position + MathHelper.ConvertHeadingToDirection(heading) * FallbackRoadWidth;
        }
    }
}