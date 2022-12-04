using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Models;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Street;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.CloseRoad
{
    public class CloseRoadDispatcher : ICloseRoadDispatcher
    {
        private const string AudioAccepted = "ROADBLOCK_ACCEPTED";
        private const string AudioNegative = "ROADBLOCK_NEGATIVE";
        private const string AudioCloseRoad = "ROADBLOCK_CLOSE_ROAD";

        private readonly ILogger _logger;
        private readonly IGame _game;
        private readonly ISettingsManager _settingsManager;
        private readonly IModelProvider _modelProvider;
        private readonly ILocalizer _localizer;

        private readonly List<IRoadClosure> _instances = new();
        private bool _active;

        public CloseRoadDispatcher(ILogger logger, IGame game, ISettingsManager settingsManager, IModelProvider modelProvider, ILocalizer localizer)
        {
            _logger = logger;
            _game = game;
            _settingsManager = settingsManager;
            _modelProvider = modelProvider;
            _localizer = localizer;
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
        public IRoadClosure CloseNearbyRoad(Vector3 position, bool preview = false)
        {
            return CloseNearbyRoad(position, BackupUnit, _modelProvider.TryFindModelByScriptName<BarrierModel>(_settingsManager.CloseRoadSettings.Barrier),
                LightSource, MaxDistance, preview);
        }

        /// <inheritdoc />
        public IRoadClosure CloseNearbyRoad(Vector3 position, EBackupUnit backupUnit, BarrierModel barrier, LightModel lightSource, float maxDistance,
            bool preview = false)
        {
            try
            {
                var closestNode = RoadQuery.FindClosestRoad(position, EVehicleNodeType.AllNodes);
                _logger.Trace($"Closing the road for node {closestNode}");

                var instance = new RoadClosureInstance(closestNode, backupUnit, barrier, lightSource, MaxDistance);
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

                LspdfrUtils.PlayScannerAudioNonBlocking($"{AudioAccepted} {AudioCloseRoad}");
                _game.DisplayNotificationDebug(_localizer[LocalizationKey.RoadClosed]);
                return instance;
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while closing the road, {ex.Message}", ex);
                LspdfrUtils.PlayScannerAudioNonBlocking($"{AudioNegative}");
            }

            return null;
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
            _active = false;
        }

        #endregion

        #region Functions

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            StartKeyListener();
        }

        private void StartKeyListener()
        {
            _active = true;
            _game.NewSafeFiber(() =>
            {
                _logger.Debug("Close road key listener has been started");
                while (_active)
                {
                    _game.FiberYield();

                    try
                    {
                        var settings = _settingsManager.CloseRoadSettings;
                        if (GameUtils.IsKeyPressed(settings.CloseRoadKey, settings.CloseRoadModifierKey))
                        {
                            CloseNearbyRoad(_game.PlayerPosition);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"An error occurred while processing the close road key listener, {ex.Message}", ex);
                    }
                }
                _logger.Info("Close road key listener has been stopped");
            }, $"{GetType()}.KeyListener");
        }

        #endregion
    }
}