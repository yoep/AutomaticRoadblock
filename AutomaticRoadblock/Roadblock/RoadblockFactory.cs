using System;
using System.Collections.Generic;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    public static class RoadblockFactory
    {
        private static readonly Dictionary<RoadblockLevel, Func<Road, Vehicle, bool, bool, IRoadblock>> Roadblocks =
            new()
            {
                {
                    RoadblockLevel.Level1,
                    (road, vehicle, limitSpeed, shouldAddLights) => new RoadblockLevel1(road, vehicle, limitSpeed, shouldAddLights)
                },
                {
                    RoadblockLevel.Level2,
                    (road, vehicle, limitSpeed, shouldAddLights) => new RoadblockLevel2(road, vehicle, limitSpeed, shouldAddLights)
                },
                {
                    RoadblockLevel.Level3,
                    (road, vehicle, limitSpeed, shouldAddLights) => new RoadblockLevel3(road, vehicle, limitSpeed, shouldAddLights)
                },
                {
                    RoadblockLevel.Level4,
                    (road, vehicle, limitSpeed, shouldAddLights) => new RoadblockLevel4(road, vehicle, limitSpeed, shouldAddLights)
                },
                {
                    RoadblockLevel.Level5,
                    (road, vehicle, limitSpeed, shouldAddLights) => new RoadblockLevel5(road, vehicle, limitSpeed, shouldAddLights)
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