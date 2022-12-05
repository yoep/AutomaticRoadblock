using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Models;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Street;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.ManualPlacement
{
    internal class ManualPlacement : AbstractInstancePlacementManager<ManualRoadblock>, IManualPlacement
    {
        private readonly ISettingsManager _settingsManager;

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
            _mainBarrier = modelProvider.TryFindModelByScriptName<BarrierModel>(settingsManager.ManualPlacementSettings.DefaultMainBarrier);
            _secondaryBarrier = modelProvider.TryFindModelByScriptName<BarrierModel>(settingsManager.ManualPlacementSettings.DefaultSecondaryBarrier);
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
            PlaceRoadblock(Game.PlayerPosition + MathHelper.ConvertHeadingToDirection(Game.PlayerHeading) * DistanceInFrontOfPlayer);
        }

        /// <inheritdoc />
        public IRoadblock PlaceRoadblock(Vector3 position)
        {
            ManualRoadblock roadblockToSpawn;

            lock (Instances)
            {
                roadblockToSpawn = Instances.FirstOrDefault(x => x.IsPreviewActive)
                                   ?? CreateInstance(RoadQuery.ToVehicleNode(LastDeterminedStreet ?? CalculateNewLocationForInstance(position)));
            }

            return TrackInstanceAndSpawn(roadblockToSpawn);
        }

        public IRoadblock PlaceRoadblock(Vector3 position, float targetHeading, EBackupUnit backupType, BarrierModel mainBarrier, BarrierModel secondaryBarrier,
            LightModel lightSource, PlacementType placementType, bool copsEnabled, float offset)
        {
            var node = RoadQuery.ToVehicleNode(CalculateNewLocationForInstance(position));
            var roadblockToSpawn = DoInternalInstanceCreation(node, targetHeading, mainBarrier, secondaryBarrier, backupType, placementType, lightSource,
                copsEnabled, offset);

            return TrackInstanceAndSpawn(roadblockToSpawn);
        }

        /// <inheritdoc />
        public void RemoveRoadblocks(RemoveType removeType)
        {
            DoInternalInstanceRemoval(removeType);
        }

        /// <inheritdoc />
        public void Remove(IRoadblock roadblock)
        {
            if (roadblock.GetType() == typeof(ManualRoadblock))
            {
                DisposeInstance((ManualRoadblock)roadblock);
            }
            else
            {
                Logger.Warn($"Unable to remove manual placed roadblock, invalid instance type {roadblock.GetType()}");
            }
        }

        #endregion

        #region Functions

        protected override ManualRoadblock CreateInstance(IVehicleNode node)
        {
            Assert.NotNull(node, "road cannot be null");
            if (node.GetType() == typeof(Intersection))
                return null;

            var targetHeading = Direction == PlacementDirection.Towards ? Game.PlayerHeading : Game.PlayerHeading - 180;
            return DoInternalInstanceCreation(node, targetHeading, _mainBarrier, _secondaryBarrier, _backupType, _placementType, LightSourceType, _copsEnabled,
                _offset);
        }

        /// <inheritdoc />
        protected override Vector3 CalculatePreviewPosition()
        {
            return Game.PlayerPosition + MathHelper.ConvertHeadingToDirection(Game.PlayerHeading) * _settingsManager.ManualPlacementSettings.DistanceFromPlayer;
        }

        private ManualRoadblock DoInternalInstanceCreation(IVehicleNode node, float targetHeading, BarrierModel mainBarrier, BarrierModel secondaryBarrier,
            EBackupUnit backupType, PlacementType placementType, LightModel lightSource, bool copsEnabled, float offset)
        {
            var request = new ManualRoadblock.Request
            {
                Road = (Road)node,
                MainBarrier = mainBarrier,
                SecondaryBarrier = secondaryBarrier,
                BackupType = backupType,
                PlacementType = placementType,
                TargetHeading = targetHeading,
                AddLights = lightSource != LightModel.None,
                LightSources = new List<LightModel> { lightSource },
                CopsEnabled = copsEnabled,
                Offset = offset
            };

            Logger.Trace($"Creating new manual roadblock for request {request}");
            var roadblock = new ManualRoadblock(request);
            Logger.Debug($"Created manual roadblock {roadblock}");
            return roadblock;
        }

        private IRoadblock TrackInstanceAndSpawn(ManualRoadblock roadblockToSpawn)
        {
            lock (Instances)
            {
                Instances.Add(roadblockToSpawn);
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

            return roadblockToSpawn;
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

        #endregion
    }
}