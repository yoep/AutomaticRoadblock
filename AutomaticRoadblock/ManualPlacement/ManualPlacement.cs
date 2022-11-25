using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Models;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Street;
using AutomaticRoadblocks.Street.Info;

namespace AutomaticRoadblocks.ManualPlacement
{
    internal class ManualPlacement : AbstractInstancePlacementManager<ManualRoadblock>, IManualPlacement
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IModelProvider _modelProvider;

        private BarrierModel _mainBarrier;
        private BarrierModel _secondaryBarrier;
        private EBackupUnit _backupType = EBackupUnit.LocalPatrol;
        private LightModel _lightSourceType = LightModel.None;
        private PlacementType _placementType = PlacementType.All;
        private PlacementDirection _direction = PlacementDirection.Towards;
        private bool _copsEnabled;
        private float _offset;

        public ManualPlacement(ILogger logger, IGame game, ISettingsManager settingsManager, IModelProvider modelProvider)
            : base(game, logger)
        {
            _settingsManager = settingsManager;
            _modelProvider = modelProvider;
            _mainBarrier = BarrierModelFromSettings(settingsManager.ManualPlacementSettings.DefaultMainBarrier);
            _secondaryBarrier = BarrierModelFromSettings(settingsManager.ManualPlacementSettings.DefaultSecondaryBarrier);
            _copsEnabled = settingsManager.ManualPlacementSettings.EnableCops;
        }

        #region Properties

        /// <inheritdoc />
        public BarrierModel MainBarrier
        {
            get => _mainBarrier;
            set => UpdateMainBarrier(value);
        }

        /// <inheritdoc />
        public BarrierModel SecondaryBarrier
        {
            get => _secondaryBarrier;
            set => UpdateSecondaryBarrier(value);
        }

        /// <inheritdoc />
        public EBackupUnit BackupType
        {
            get => _backupType;
            set => UpdateBackupUnit(value);
        }

        /// <inheritdoc />
        public LightModel LightSourceType
        {
            get => _lightSourceType;
            set => UpdateLightSource(value);
        }

        /// <inheritdoc />
        public PlacementType PlacementType
        {
            get => _placementType;
            set => UpdatePlacementType(value);
        }

        /// <inheritdoc />
        public PlacementDirection Direction
        {
            get => _direction;
            set => UpdateDirection(value);
        }

        /// <inheritdoc />
        public bool CopsEnabled
        {
            get => _copsEnabled;
            set => UpdateCopsEnabled(value);
        }

        /// <inheritdoc />
        public float Offset
        {
            get => _offset;
            set => UpdateOffset(value);
        }

        /// <inheritdoc />
        protected override bool IsHologramPreviewEnabled => _settingsManager.ManualPlacementSettings.EnablePreview;

        /// <inheritdoc />
        protected override float DistanceInFrontOfPlayer => _settingsManager.ManualPlacementSettings.DistanceFromPlayer;

        #endregion

        #region Methods

        /// <inheritdoc />
        public void PlaceRoadblock()
        {
            ManualRoadblock roadblockToSpawn;

            lock (Instances)
            {
                roadblockToSpawn = Instances.FirstOrDefault(x => x.IsPreviewActive);

                if (roadblockToSpawn == null)
                {
                    roadblockToSpawn = CreateInstance(RoadQuery.ToVehicleNode(LastDeterminedStreet ?? CalculateNewLocationForInstance()));
                    Instances.Add(roadblockToSpawn);
                }
            }

            Logger.Trace($"Spawning manual roadblock {roadblockToSpawn}");
            var success = roadblockToSpawn.Spawn();

            if (success)
            {
                Logger.Info($"Manual roadblock has been spawned with success, {roadblockToSpawn}");
            }
            else
            {
                Logger.Warn($"Manual roadblock was unable to be spawned correctly, {roadblockToSpawn}");
            }
        }

        /// <inheritdoc />
        public void RemoveRoadblocks(RemoveType removeType)
        {
            DoInternalInstanceRemoval(removeType);
        }

        #endregion

        #region Functions

        protected override ManualRoadblock CreateInstance(IVehicleNode node)
        {
            Assert.NotNull(node, "road cannot be null");
            if (node.GetType() == typeof(Intersection))
                return null;

            var targetHeading = Direction == PlacementDirection.Towards ? Game.PlayerHeading : Game.PlayerHeading - 180;
            var request = new ManualRoadblock.Request
            {
                Road = (Road)node,
                MainBarrier = _mainBarrier,
                SecondaryBarrier = _secondaryBarrier,
                BackupType = _backupType,
                PlacementType = _placementType,
                TargetHeading = targetHeading,
                AddLights = LightSourceType != LightModel.None,
                LightSources = new List<LightModel> { LightSourceType },
                CopsEnabled = CopsEnabled,
                Offset = Offset,
            };

            Logger.Trace($"Creating new manual roadblock for request {request}");
            var roadblock = new ManualRoadblock(request);
            Logger.Debug($"Created manual roadblock {roadblock}");
            return roadblock;
        }

        private void UpdateMainBarrier(BarrierModel newType)
        {
            _mainBarrier = newType;
            DoInternalPreviewCreation(true);
        }

        private void UpdateSecondaryBarrier(BarrierModel newType)
        {
            _secondaryBarrier = newType;
            DoInternalPreviewCreation(true);
        }

        private void UpdateBackupUnit(EBackupUnit value)
        {
            _backupType = value;
            DoInternalPreviewCreation(true);
        }

        private void UpdateLightSource(LightModel lightSourceType)
        {
            _lightSourceType = lightSourceType;
            DoInternalPreviewCreation(true);
        }

        private void UpdatePlacementType(PlacementType placementType)
        {
            _placementType = placementType;
            DoInternalPreviewCreation(true);
        }

        private void UpdateCopsEnabled(bool copsEnabled)
        {
            _copsEnabled = copsEnabled;
            DoInternalPreviewCreation(true);
        }

        private void UpdateOffset(float value)
        {
            _offset = value;
            DoInternalPreviewCreation(true);
        }

        private void UpdateDirection(PlacementDirection value)
        {
            _direction = value;
            DoInternalPreviewCreation(true);
        }

        private BarrierModel BarrierModelFromSettings(string defaultBarrier)
        {
            Logger.Debug($"Using barrier {defaultBarrier} for manual placement if found");
            return _modelProvider.BarrierModels
                .Where(x => string.Equals(x.ScriptName, defaultBarrier, StringComparison.OrdinalIgnoreCase))
                .DefaultIfEmpty(BarrierModel.None)
                .First();
        }

        #endregion
    }
}