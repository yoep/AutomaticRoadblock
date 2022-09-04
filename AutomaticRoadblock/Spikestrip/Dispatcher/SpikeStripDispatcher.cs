using System.Collections.Generic;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.SpikeStrip.Dispatcher
{
    public class SpikeStripDispatcher : ISpikeStripDispatcher
    {
        private const string AudioSpikeStripBypassed = "ROADBLOCK_SPIKESTRIP_BYPASSED";
        private const string AudioSpikeStripHit = "ROADBLOCK_SPIKESTRIP_HIT";
        private const string AudioSpikeStripDeployedLeft = "ROADBLOCK_SPIKESTRIP_DEPLOYED_LEFT";
        private const string AudioSpikeStripDeployedMiddle = "ROADBLOCK_SPIKESTRIP_DEPLOYED_MIDDLE";
        private const string AudioSpikeStripDeployedRight = "ROADBLOCK_SPIKESTRIP_DEPLOYED_RIGHT";

        private readonly ILogger _logger;
        private readonly List<ISpikeStrip> _spikeStrips = new();

        public SpikeStripDispatcher(ILogger logger)
        {
            _logger = logger;
        }

        #region ISpikeStripDispatcher

        /// <inheritdoc />
        public void Deploy(Vector3 position)
        {
            var road = RoadUtils.FindClosestRoad(position, ERoadType.All);

            lock (_spikeStrips)
            {
                var spikeStrip = new SpikeStrip(road, ESpikeStripLocation.Right);
                
                spikeStrip.StateChanged += SpikeStripStateChanged;
                spikeStrip.Deploy();
                
                _spikeStrips.Add(spikeStrip);
                _logger.Info($"Spawned spike strip {spikeStrip}");
            }
        }

        /// <inheritdoc />
        public void RemoveAll()
        {
            Dispose();
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            lock (_spikeStrips)
            {
                _spikeStrips.ForEach(x=>x.Dispose());
                _spikeStrips.Clear();
            }
        }

        #endregion
        
        #region Functions

        private void SpikeStripStateChanged(ISpikeStrip spikeStrip, ESpikeStripState state)
        {
            _logger.Debug($"Spike strip state changed to {state} for {spikeStrip}");
        }
        
        #endregion
    }
}