using System;
using System.Collections.Generic;
using AutomaticRoadblocks.Pursuit.Level;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Factory
{
    internal static class PursuitRoadblockFactory
    {
        private static readonly Dictionary<ERoadblockLevel, Func<Road, Vehicle, ERoadblockFlags, IRoadblock>> Roadblocks =
            new()
            {
                {
                    ERoadblockLevel.Level1,
                    (road, vehicle, flags) =>
                        new PursuitRoadblockLevel1(road, vehicle, flags)
                },
                {
                    ERoadblockLevel.Level2,
                    (road, vehicle, flags) =>
                        new PursuitRoadblockLevel2(road, vehicle, flags)
                },
                {
                    ERoadblockLevel.Level3,
                    (road, vehicle, flags) =>
                        new PursuitRoadblockLevel3(road, vehicle, flags)
                },
                {
                    ERoadblockLevel.Level4,
                    (road, vehicle, flags) =>
                        new PursuitRoadblockLevel4(road, vehicle, flags)
                },
                {
                    ERoadblockLevel.Level5,
                    (road, vehicle, flags) =>
                        new PursuitRoadblockLevel5(road, vehicle, flags)
                }
            };

        internal static IRoadblock Create(ERoadblockLevel level, Road street, Vehicle vehicle,
            ERoadblockFlags flags)
        {
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(street, "road cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");
            return Roadblocks[level].Invoke(street, vehicle, flags);
        }
    }
}