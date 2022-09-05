using System.Collections.Generic;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using JetBrains.Annotations;
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
            return DoInternalSpikeStripCreation(road, stripLocation, DeploymentType.Spawn);
        }

        /// <inheritdoc />
        public ISpikeStrip Spawn(Road road, Road.Lane lane, ESpikeStripLocation stripLocation, Vehicle targetVehicle)
        {
            return DoInternalSpikeStripCreation(road, stripLocation, DeploymentType.Spawn, targetVehicle);
        }

        /// <inheritdoc />
        public ISpikeStrip Deploy(Vector3 position, ESpikeStripLocation stripLocation)
        {
            var road = RoadUtils.FindClosestRoad(position, ERoadType.All);
            return DoInternalSpikeStripCreation(road, stripLocation, DeploymentType.Deploy);
        }

        /// <inheritdoc />
        public ISpikeStrip Deploy(Road road, ESpikeStripLocation stripLocation)
        {
            return DoInternalSpikeStripCreation(road, stripLocation, DeploymentType.Deploy);
        }

        public ISpikeStrip Deploy(Road road, ESpikeStripLocation stripLocation, Vehicle targetVehicle)
        {
            return DoInternalSpikeStripCreation(road, stripLocation, DeploymentType.Deploy, targetVehicle);
        }

        /// <inheritdoc />
        public ISpikeStrip CreatePreview(Vector3 position, ESpikeStripLocation stripLocation)
        {
            var road = RoadUtils.FindClosestRoad(position, ERoadType.All);
            return DoInternalSpikeStripCreation(road, stripLocation, DeploymentType.Preview);
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
                _spikeStrips.ForEach(x =>
                {
                    x.DeletePreview();
                    x.Dispose();
                });
                _spikeStrips.Clear();
            }
        }

        #endregion

        #region Functions

        private ISpikeStrip DoInternalSpikeStripCreation(Road road, ESpikeStripLocation stripLocation, DeploymentType type, Vehicle targetVehicle = null)
        {
            return DoInternalSpikeStripCreation(road, null, stripLocation, type, targetVehicle);
        }

        private ISpikeStrip DoInternalSpikeStripCreation(Road road, [CanBeNull] Road.Lane lane, ESpikeStripLocation stripLocation, DeploymentType type,
            Vehicle targetVehicle = null)
        {
            ISpikeStrip spikeStrip;

            lock (_spikeStrips)
            {
                spikeStrip = targetVehicle == null
                    ? DoSpikeStripCreation(road, stripLocation)
                    : DoPursuitSpikeStripCreation(road, lane, stripLocation, targetVehicle);
                _spikeStrips.Add(spikeStrip);
            }

            spikeStrip.StateChanged += SpikeStripStateChanged;

            switch (type)
            {
                case DeploymentType.Deploy:
                    spikeStrip.Deploy();
                    break;
                case DeploymentType.Spawn:
                    spikeStrip.Spawn();
                    break;
                case DeploymentType.Preview:
                    spikeStrip.CreatePreview();
                    break;
            }

            _logger.Info($"Spawned spike strip {spikeStrip}");
            return spikeStrip;
        }

        private void SpikeStripStateChanged(ISpikeStrip spikeStrip, ESpikeStripState state)
        {
            _logger.Debug($"Spike strip state changed to {state} for {spikeStrip}");
            switch (state)
            {
                case ESpikeStripState.Deploying:
                    LspdfrUtils.PlayScannerAudio(GetAudioName(spikeStrip.Location));
                    break;
                case ESpikeStripState.Hit:
                    LspdfrUtils.PlayScannerAudio(AudioSpikeStripHit);
                    break;
                case ESpikeStripState.Bypassed:
                    LspdfrUtils.PlayScannerAudio(AudioSpikeStripBypassed);
                    break;
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

        private static SpikeStrip DoSpikeStripCreation(Road road, ESpikeStripLocation stripLocation)
        {
            return new SpikeStrip(road, stripLocation);
        }

        private static PursuitSpikeStrip DoPursuitSpikeStripCreation(Road road, [CanBeNull] Road.Lane lane, ESpikeStripLocation stripLocation,
            Vehicle targetVehicle)
        {
            return lane == null
                ? new PursuitSpikeStrip(road, stripLocation, targetVehicle)
                : new PursuitSpikeStrip(road, lane, stripLocation, targetVehicle);
        }

        #endregion

        private enum DeploymentType
        {
            Spawn,
            Deploy,
            Preview
        }
    }
}