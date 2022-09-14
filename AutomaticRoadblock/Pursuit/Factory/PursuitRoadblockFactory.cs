using System;
using System.Collections.Generic;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Pursuit.Level;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Factory
{
    internal static class PursuitRoadblockFactory
    {
        private static readonly Dictionary<ERoadblockLevel, Func<Road, BarrierModel, BarrierModel, Vehicle, ERoadblockFlags, IRoadblock>> Roadblocks =
            new()
            {
                {
                    ERoadblockLevel.Level1,
                    (road, mainBarrier, _, vehicle, flags) =>
                        new PursuitRoadblockLevel1(road, mainBarrier, vehicle, flags)
                },
                {
                    ERoadblockLevel.Level2,
                    (road, mainBarrier, _, vehicle, flags) =>
                        new PursuitRoadblockLevel2(road, mainBarrier, vehicle, flags)
                },
                {
                    ERoadblockLevel.Level3,
                    (road, mainBarrier, _, vehicle, flags) =>
                        new PursuitRoadblockLevel3(road, mainBarrier, vehicle, flags)
                },
                {
                    ERoadblockLevel.Level4,
                    (road, mainBarrier, chaseVehicleBarrier, vehicle, flags) =>
                        new PursuitRoadblockLevel4(road, mainBarrier, chaseVehicleBarrier, vehicle, flags)
                },
                {
                    ERoadblockLevel.Level5,
                    (road, mainBarrier, chaseVehicleBarrier, vehicle, flags) =>
                        new PursuitRoadblockLevel5(road, mainBarrier, chaseVehicleBarrier, vehicle, flags)
                }
            };

        internal static IRoadblock Create(ERoadblockLevel level, Road street, BarrierModel mainBarrier, BarrierModel chaseVehicleBarrier, Vehicle vehicle,
            ERoadblockFlags flags)
        {
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(mainBarrier, "mainBarrier cannot be null");
            Assert.NotNull(chaseVehicleBarrier, "chaseVehicleBarrier cannot be null");
            Assert.NotNull(street, "road cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");

            return Roadblocks[level].Invoke(street, mainBarrier, chaseVehicleBarrier, vehicle, flags);
        }
    }
}