using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Models;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Street;
using Rage;

namespace AutomaticRoadblocks.CloseRoad
{
    public class CloseRoadDispatcher : ICloseRoadDispatcher
    {
        private readonly ILogger _logger;
        private readonly ISettingsManager _settingsManager;
        private readonly IModelProvider _modelProvider;

        private readonly List<CloseRoadInstance> _instances = new();

        public CloseRoadDispatcher(ILogger logger, ISettingsManager settingsManager, IModelProvider modelProvider)
        {
            _logger = logger;
            _settingsManager = settingsManager;
            _modelProvider = modelProvider;
        }

        #region Properties

        /// <inheritdoc />
        public EBackupUnit BackupUnit { get; set; } = EBackupUnit.LocalPatrol;

        /// <inheritdoc />
        public LightModel LightSource { get; set; } = LightModel.None;

        private float MaxDistance => _settingsManager.CloseRoadSettings.MaxDistanceFromPlayer;

        #endregion

        #region ICloseRoadDispatcher

        /// <inheritdoc />
        public ICloseRoad CloseNearbyRoad(Vector3 position, bool preview = false)
        {
            return CloseNearbyRoad(position, BackupUnit, _modelProvider.TryFindModelByScriptName<BarrierModel>(_settingsManager.CloseRoadSettings.Barrier),
                LightSource, MaxDistance, preview);
        }

        /// <inheritdoc />
        public ICloseRoad CloseNearbyRoad(Vector3 position, EBackupUnit backupUnit, BarrierModel barrier, LightModel lightSource, float maxDistance,
            bool preview = false)
        {
            var closestNode = RoadQuery.FindClosestRoad(position, EVehicleNodeType.AllNodes);
            _logger.Trace($"Closing the road for node {closestNode}");

            var instance = new CloseRoadInstance(closestNode, backupUnit, barrier, lightSource, MaxDistance);
            lock (_instances)
            {
                _instances.Add(instance);
            }

            if (preview)
            {
                instance.CreatePreview();
            }
            else
            {
                instance.Spawn();
            }

            return instance;
        }

        /// <inheritdoc />
        public void OpenRoads(bool previewsOnly = false)
        {
            lock (_instances)
            {
                var instancesToRemove = _instances
                    .Where(x => x.IsPreviewActive == previewsOnly)
                    .ToList();

                instancesToRemove.ForEach(x =>
                {
                    x.Release();
                    x.Dispose();
                    _instances.Remove(x);
                });
            }
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _instances.ForEach(x => x.Dispose());
            _instances.Clear();
        }

        #endregion
    }
}