using System;
using System.Collections.Generic;
using AutomaticRoadblocks.Pursuit.Level;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.SpikeStrip.Dispatcher;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Factory
{
    internal static class PursuitRoadblockFactory
    {
        private static readonly Dictionary<ERoadblockLevel, Func<ISpikeStripDispatcher, Road, Vehicle, ERoadblockFlags, IRoadblock>> Roadblocks =
            new()
            {
                {
                    ERoadblockLevel.Level1,
                    (spikeStripDispatcher, road, vehicle, flags) =>
                        new PursuitRoadblockLevel1(spikeStripDispatcher, road, vehicle, flags)
                },
                {
                    ERoadblockLevel.Level2,
                    (spikeStripDispatcher, road, vehicle, flags) =>
                        new PursuitRoadblockLevel2(spikeStripDispatcher, road, vehicle, flags)
                },
                {
                    ERoadblockLevel.Level3,
                    (spikeStripDispatcher, road, vehicle, flags) =>
                        new PursuitRoadblockLevel3(spikeStripDispatcher, road, vehicle, flags)
                },
                {
                    ERoadblockLevel.Level4,
                    (spikeStripDispatcher, road, vehicle, flags) =>
                        new PursuitRoadblockLevel4(spikeStripDispatcher, road, vehicle, flags)
                },
                {
                    ERoadblockLevel.Level5,
                    (spikeStripDispatcher, road, vehicle, flags) =>
                        new PursuitRoadblockLevel5(spikeStripDispatcher, road, vehicle, flags)
                }
            };

        internal static IRoadblock Create(ISpikeStripDispatcher spikeStripDispatcher, ERoadblockLevel level, Road street, Vehicle vehicle,
            ERoadblockFlags flags)
        {
            Assert.NotNull(spikeStripDispatcher, "spikeStripDispatcher cannot be null");
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(street, "road cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");
            return Roadblocks[level].Invoke(spikeStripDispatcher, street, vehicle, flags);
        }
    }
}