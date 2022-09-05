using System;
using System.Collections.Generic;
using AutomaticRoadblocks.Pursuit.Level;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.SpikeStrip.Dispatcher;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Factory
{
    internal static class PursuitRoadblockFactory
    {
        private static readonly Dictionary<RoadblockLevel, Func<ISpikeStripDispatcher, Road, Vehicle, bool, bool, bool, IRoadblock>> Roadblocks =
            new()
            {
                {
                    RoadblockLevel.Level1,
                    (spikeStripDispatcher, road, vehicle, limitSpeed, shouldAddLights, addSpikeStrip) =>
                        new PursuitRoadblockLevel1(spikeStripDispatcher, road, vehicle, limitSpeed, shouldAddLights, addSpikeStrip)
                },
                {
                    RoadblockLevel.Level2,
                    (spikeStripDispatcher, road, vehicle, limitSpeed, shouldAddLights, addSpikeStrip) =>
                        new PursuitRoadblockLevel2(spikeStripDispatcher, road, vehicle, limitSpeed, shouldAddLights, addSpikeStrip)
                },
                {
                    RoadblockLevel.Level3,
                    (spikeStripDispatcher, road, vehicle, limitSpeed, shouldAddLights, addSpikeStrip) =>
                        new PursuitRoadblockLevel3(spikeStripDispatcher, road, vehicle, limitSpeed, shouldAddLights, addSpikeStrip)
                },
                {
                    RoadblockLevel.Level4,
                    (spikeStripDispatcher, road, vehicle, limitSpeed, shouldAddLights, addSpikeStrip) =>
                        new PursuitRoadblockLevel4(spikeStripDispatcher, road, vehicle, limitSpeed, shouldAddLights, addSpikeStrip)
                },
                {
                    RoadblockLevel.Level5,
                    (spikeStripDispatcher, road, vehicle, limitSpeed, shouldAddLights, addSpikeStrip) =>
                        new PursuitRoadblockLevel5(spikeStripDispatcher, road, vehicle, limitSpeed, shouldAddLights, addSpikeStrip)
                }
            };

        internal static IRoadblock Create(ISpikeStripDispatcher spikeStripDispatcher, RoadblockLevel level, Road road, Vehicle vehicle, bool limitSpeed,
            bool shouldAddLights, bool addSpikeStrip)
        {
            Assert.NotNull(spikeStripDispatcher, "spikeStripDispatcher cannot be null");
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(road, "road cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");
            return Roadblocks[level].Invoke(spikeStripDispatcher, road, vehicle, limitSpeed, shouldAddLights, addSpikeStrip);
        }
    }
}