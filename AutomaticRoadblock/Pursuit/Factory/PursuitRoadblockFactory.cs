using System;
using System.Collections.Generic;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Pursuit.Level;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Factory
{
    internal static class PursuitRoadblockFactory
    {
        private static readonly
            Dictionary<ERoadblockLevel, Func<Road, BarrierModel, BarrierModel, BarrierModel, Vehicle, List<LightModel>, ERoadblockFlags, IRoadblock>>
            Roadblocks =
                new()
                {
                    {
                        ERoadblockLevel.Level1,
                        (road, mainBarrier, secondaryBarrier, _, vehicle, lightSources, flags) =>
                            new PursuitRoadblockLevel1(road, mainBarrier, secondaryBarrier, vehicle, lightSources, flags)
                    },
                    {
                        ERoadblockLevel.Level2,
                        (road, mainBarrier, secondaryBarrier, _, vehicle, lightSources, flags) =>
                            new PursuitRoadblockLevel2(road, mainBarrier, secondaryBarrier, vehicle, lightSources, flags)
                    },
                    {
                        ERoadblockLevel.Level3,
                        (road, mainBarrier, secondaryBarrier, _, vehicle, lightSources, flags) =>
                            new PursuitRoadblockLevel3(road, mainBarrier, secondaryBarrier, vehicle, lightSources, flags)
                    },
                    {
                        ERoadblockLevel.Level4,
                        (road, mainBarrier, secondaryBarrier, chaseVehicleBarrier, vehicle, lightSources, flags) =>
                            new PursuitRoadblockLevel4(road, mainBarrier, secondaryBarrier, chaseVehicleBarrier, vehicle, lightSources, flags)
                    },
                    {
                        ERoadblockLevel.Level5,
                        (road, mainBarrier, secondaryBarrier, chaseVehicleBarrier, vehicle, lightSources, flags) =>
                            new PursuitRoadblockLevel5(road, mainBarrier, secondaryBarrier, chaseVehicleBarrier, vehicle, lightSources, flags)
                    }
                };

        internal static IRoadblock Create(ERoadblockLevel level, Road street, BarrierModel mainBarrier, BarrierModel secondaryBarrier,
            BarrierModel chaseVehicleBarrier, Vehicle vehicle, List<LightModel> lightSources, ERoadblockFlags flags)
        {
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(mainBarrier, "mainBarrier cannot be null");
            Assert.NotNull(secondaryBarrier, "secondaryBarrier cannot be null");
            Assert.NotNull(chaseVehicleBarrier, "chaseVehicleBarrier cannot be null");
            Assert.NotNull(street, "road cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");
            Assert.NotNull(lightSources, "lightSources cannot be null");

            return Roadblocks[level].Invoke(street, mainBarrier, secondaryBarrier, chaseVehicleBarrier, vehicle, lightSources, flags);
        }
    }
}