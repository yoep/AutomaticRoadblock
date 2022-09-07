using System.Collections.Generic;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Utils.Road;
using JetBrains.Annotations;
using Rage;

namespace AutomaticRoadblocks.SpikeStrip.Dispatcher
{
    public class SpikeStripDispatcher : ISpikeStripDispatcher
    {
        private readonly ILogger _logger;
        private readonly List<ISpikeStrip> _spikeStrips = new();

        public SpikeStripDispatcher(ILogger logger)
        {
            _logger = logger;
        }

        #region ISpikeStripDispatcher

        /// <inheritdoc />
        public ISpikeStrip Spawn(Road road, ESpikeStripLocation stripLocation, float offset = 0f)
        {
            return DoInternalSpikeStripCreation(road, null, stripLocation, DeploymentType.Spawn, null, offset);
        }

        /// <inheritdoc />
        public ISpikeStrip Spawn(Road road, Road.Lane lane, ESpikeStripLocation stripLocation, Vehicle targetVehicle, float offset = 0f)
        {
            return DoInternalSpikeStripCreation(road, lane, stripLocation, DeploymentType.Spawn, targetVehicle, offset);
        }

        /// <inheritdoc />
        public ISpikeStrip Deploy(Vector3 position, ESpikeStripLocation stripLocation)
        {
            var road = RoadUtils.FindClosestRoad(position, ERoadType.All);
            return DoInternalSpikeStripCreation(road, null, stripLocation, DeploymentType.Deploy, null, 0f);
        }

        /// <inheritdoc />
        public ISpikeStrip Deploy(Road road, ESpikeStripLocation stripLocation)
        {
            return DoInternalSpikeStripCreation(road, null, stripLocation, DeploymentType.Deploy, null, 0f);
        }

        public ISpikeStrip Deploy(Road road, ESpikeStripLocation stripLocation, Vehicle targetVehicle)
        {
            return DoInternalSpikeStripCreation(road, null, stripLocation, DeploymentType.Deploy, targetVehicle, 0f);
        }

        /// <inheritdoc />
        public ISpikeStrip CreatePreview(Vector3 position, ESpikeStripLocation stripLocation)
        {
            var road = RoadUtils.FindClosestRoad(position, ERoadType.All);
            return DoInternalSpikeStripCreation(road, null, stripLocation, DeploymentType.Preview, null, 0f);
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

        private ISpikeStrip DoInternalSpikeStripCreation(Road road, [CanBeNull] Road.Lane lane, ESpikeStripLocation stripLocation, DeploymentType type,
            [CanBeNull] Vehicle targetVehicle, float offset)
        {
            ISpikeStrip spikeStrip;

            lock (_spikeStrips)
            {
                spikeStrip = targetVehicle == null
                    ? DoSpikeStripCreation(road, stripLocation, offset)
                    : DoPursuitSpikeStripCreation(road, lane, stripLocation, targetVehicle, offset);
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
        }

        private static SpikeStrip DoSpikeStripCreation(Road road, ESpikeStripLocation stripLocation, float offset)
        {
            return new SpikeStrip(road, stripLocation, offset);
        }

        private static PursuitSpikeStrip DoPursuitSpikeStripCreation(Road road, [CanBeNull] Road.Lane lane, ESpikeStripLocation stripLocation,
            Vehicle targetVehicle, float offset)
        {
            return lane == null
                ? new PursuitSpikeStrip(road, stripLocation, targetVehicle, offset)
                : new PursuitSpikeStrip(road, lane, stripLocation, targetVehicle, offset);
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