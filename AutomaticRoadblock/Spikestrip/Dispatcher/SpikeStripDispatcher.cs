using System.Collections.Generic;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Utils;
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
        public ISpikeStrip Spawn(Road road, ESpikeStripLocation stripLocation)
        {
            return DoInternalSpikeStripCreation(road, stripLocation, false);
        }

        /// <inheritdoc />
        public ISpikeStrip Deploy(Vector3 position, ESpikeStripLocation stripLocation)
        {
            var road = RoadUtils.FindClosestRoad(position, ERoadType.All);
            return DoInternalSpikeStripCreation(road, stripLocation, true);
        }

        /// <inheritdoc />
        public ISpikeStrip Deploy(Road road, ESpikeStripLocation stripLocation)
        {
            return DoInternalSpikeStripCreation(road, stripLocation, true);
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
                _spikeStrips.ForEach(x => x.Dispose());
                _spikeStrips.Clear();
            }
        }

        #endregion

        #region Functions

        private ISpikeStrip DoInternalSpikeStripCreation(Road road, ESpikeStripLocation stripLocation, bool deploy)
        {
            ISpikeStrip spikeStrip;

            lock (_spikeStrips)
            {
                spikeStrip = new SpikeStrip(road, stripLocation);
                _spikeStrips.Add(spikeStrip);
            }

            spikeStrip.StateChanged += SpikeStripStateChanged;
            
            // verify the way the spike strip should be created
            if (deploy)
            {
                spikeStrip.Deploy();
            }
            else
            {
                spikeStrip.Spawn();
            }

            _logger.Info($"Spawned spike strip {spikeStrip}");
            return spikeStrip;
        }

        private void SpikeStripStateChanged(ISpikeStrip spikeStrip, ESpikeStripState state)
        {
            _logger.Debug($"Spike strip state changed to {state} for {spikeStrip}");
            if (state == ESpikeStripState.Deploying)
            {
                LspdfrUtils.PlayScannerAudio(GetAudioName(spikeStrip.Location));
            }
        }

        private static string GetAudioName(ESpikeStripLocation stripLocation)
        {
            return stripLocation switch
            {
                ESpikeStripLocation.Left => AudioSpikeStripDeployedLeft,
                ESpikeStripLocation.Middle => AudioSpikeStripDeployedMiddle,
                _ => AudioSpikeStripDeployedRight
            };
        }

        #endregion
    }
}