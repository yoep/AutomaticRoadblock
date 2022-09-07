using System.Collections.Generic;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Street;
using AutomaticRoadblocks.Street.Info;
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
        public ISpikeStrip Spawn(Road street, ESpikeStripLocation stripLocation, float offset = 0f)
        {
            return DoInternalSpikeStripCreation(street, null, stripLocation, DeploymentType.Spawn, null, offset);
        }

        /// <inheritdoc />
        public ISpikeStrip Spawn(Road street, Road.Lane lane, ESpikeStripLocation stripLocation, Vehicle targetVehicle, float offset = 0f)
        {
            return DoInternalSpikeStripCreation(street, lane, stripLocation, DeploymentType.Spawn, targetVehicle, offset);
        }

        /// <inheritdoc />
        public ISpikeStrip Deploy(Vector3 position, ESpikeStripLocation stripLocation)
        {
            var road = (Road)RoadUtils.FindClosestRoad(position, EVehicleNodeType.AllNodes);
            return DoInternalSpikeStripCreation(road, null, stripLocation, DeploymentType.Deploy, null, 0f);
        }

        /// <inheritdoc />
        public ISpikeStrip Deploy(Road street, ESpikeStripLocation stripLocation)
        {
            return DoInternalSpikeStripCreation(street, null, stripLocation, DeploymentType.Deploy, null, 0f);
        }

        public ISpikeStrip Deploy(Road street, ESpikeStripLocation stripLocation, Vehicle targetVehicle)
        {
            return DoInternalSpikeStripCreation(street, null, stripLocation, DeploymentType.Deploy, targetVehicle, 0f);
        }

        /// <inheritdoc />
        public ISpikeStrip CreatePreview(Vector3 position, ESpikeStripLocation stripLocation)
        {
            var road = (Road)RoadUtils.FindClosestRoad(position, EVehicleNodeType.AllNodes);
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

        private ISpikeStrip DoInternalSpikeStripCreation(Road street, [CanBeNull] Road.Lane lane, ESpikeStripLocation stripLocation, DeploymentType type,
            [CanBeNull] Vehicle targetVehicle, float offset)
        {
            ISpikeStrip spikeStrip;

            lock (_spikeStrips)
            {
                spikeStrip = targetVehicle == null
                    ? DoSpikeStripCreation(street, stripLocation, offset)
                    : DoPursuitSpikeStripCreation(street, lane, stripLocation, targetVehicle, offset);
                _spikeStrips.Add(spikeStrip);
            }
            
            _logger.Debug($"Created spike strip {spikeStrip}");
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

        private static SpikeStrip DoSpikeStripCreation(Road street, ESpikeStripLocation stripLocation, float offset)
        {
            return new SpikeStrip(street, stripLocation, offset);
        }

        private static PursuitSpikeStrip DoPursuitSpikeStripCreation(Road street, [CanBeNull] Road.Lane lane, ESpikeStripLocation stripLocation,
            Vehicle targetVehicle, float offset)
        {
            return lane == null
                ? new PursuitSpikeStrip(street, stripLocation, targetVehicle, offset)
                : new PursuitSpikeStrip(street, lane, stripLocation, targetVehicle, offset);
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