using System;
using System.Collections.Generic;
using AutomaticRoadblocks.Pursuit.Level;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Factory
{
    public static class PursuitRoadblockFactory
    {
        private static readonly Dictionary<RoadblockLevel, Func<Road, Vehicle, bool, bool, IRoadblock>> Roadblocks =
            new()
            {
                {
                    RoadblockLevel.Level1,
                    (road, vehicle, limitSpeed, shouldAddLights) => new PursuitRoadblockLevel1(road, vehicle, limitSpeed, shouldAddLights)
                },
                {
                    RoadblockLevel.Level2,
                    (road, vehicle, limitSpeed, shouldAddLights) => new PursuitRoadblockLevel2(road, vehicle, limitSpeed, shouldAddLights)
                },
                {
                    RoadblockLevel.Level3,
                    (road, vehicle, limitSpeed, shouldAddLights) => new PursuitRoadblockLevel3(road, vehicle, limitSpeed, shouldAddLights)
                },
                {
                    RoadblockLevel.Level4,
                    (road, vehicle, limitSpeed, shouldAddLights) => new PursuitRoadblockLevel4(road, vehicle, limitSpeed, shouldAddLights)
                },
                {
                    RoadblockLevel.Level5,
                    (road, vehicle, limitSpeed, shouldAddLights) => new PursuitRoadblockLevel5(road, vehicle, limitSpeed, shouldAddLights)
                }
            };

        public static IRoadblock Create(RoadblockLevel level, Road road, Vehicle vehicle, bool limitSpeed, bool shouldAddLights)
        {
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(road, "road cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");
            return Roadblocks[level].Invoke(road, vehicle, limitSpeed, shouldAddLights);
        }
    }
}