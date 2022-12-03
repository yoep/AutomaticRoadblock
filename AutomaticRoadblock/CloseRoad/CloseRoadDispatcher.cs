using System;
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
        private readonly IGame _game;
        private readonly ILogger _logger;
        private readonly ISettingsManager _settingsManager;
        private readonly IModelProvider _modelProvider;

        private readonly List<CloseRoadInstance> _instances = new();

        public CloseRoadDispatcher(IGame game, ILogger logger, ISettingsManager settingsManager, IModelProvider modelProvider)
        {
            _game = game;
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
        public void CloseNearbyRoad(Vector3 position, bool preview = false)
        {
            DoInternalCloseRoad(position, preview);
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

        #region Functions

        private void DoInternalCloseRoad(Vector3 position, bool preview = false)
        {
            _game.NewSafeFiber(() =>
            {
                var closestNode = RoadQuery.FindClosestRoad(position, EVehicleNodeType.AllNodes);
                _logger.Trace($"Closing the road for node {closestNode}");

                var instance = new CloseRoadInstance(closestNode, BackupUnit, BarrierModelFromSettings(_settingsManager.CloseRoadSettings.Barrier), LightSource,
                    MaxDistance);
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
            }, $"{GetType()}.DoInternalCloseRoad");
        }

        private BarrierModel BarrierModelFromSettings(string barrier)
        {
            _logger.Debug($"Using barrier {barrier} for close road if found");
            return _modelProvider.BarrierModels
                .Where(x => string.Equals(x.ScriptName, barrier, StringComparison.OrdinalIgnoreCase))
                .DefaultIfEmpty(BarrierModel.None)
                .First();
        }

        #endregion
    }
}