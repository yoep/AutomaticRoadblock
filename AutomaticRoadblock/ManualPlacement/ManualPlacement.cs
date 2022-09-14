using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Street;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Vehicles;

namespace AutomaticRoadblocks.ManualPlacement
{
    internal class ManualPlacement : AbstractInstancePlacementManager<ManualRoadblock>, IManualPlacement
    {
        private readonly ISettingsManager _settingsManager;

        private BarrierModel _barrier = BarrierModel.None;
        private VehicleType _vehicleType = VehicleType.LocalUnit;
        private LightSourceType _lightSourceType = LightSourceType.Flares;
        private PlacementType _placementType = PlacementType.All;
        private bool _copsEnabled;
        private float _offset;

        public ManualPlacement(ILogger logger, IGame game, ISettingsManager settingsManager)
            : base(game, logger)
        {
            _settingsManager = settingsManager;
        }

        #region Properties

        /// <inheritdoc />
        public BarrierModel Barrier
        {
            get => _barrier;
            set => UpdateBarrier(value);
        }

        /// <inheritdoc />
        public VehicleType VehicleType
        {
            get => _vehicleType;
            set => UpdateVehicle(value);
        }

        /// <inheritdoc />
        public LightSourceType LightSourceType
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
        public bool CopsEnabled
        {
            get => _copsEnabled;
            set => UpdateCopsEnabled(value);
        }

        /// <inheritdoc />
        public bool SpeedLimit { get; set; }

        /// <inheritdoc />
        public float Offset
        {
            get => _offset;
            set => UpdateOffset(value);
        }

        /// <summary>
        /// Get a list of roadblocks which are previewed.
        /// <remarks>Make sure this property is called in a lock statement.</remarks>
        /// </summary>
        private List<ManualRoadblock> PreviewRoadblocks => Instances
            .Where(x => x.IsPreviewActive)
            .ToList();

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
            List<ManualRoadblock> previewRoadblocks;

            lock (Instances)
            {
                previewRoadblocks = PreviewRoadblocks;
            }

            // verify if a roadblock preview is available,
            // if so, we'll actually spawn it
            if (previewRoadblocks.Count == 1)
            {
                roadblockToSpawn = previewRoadblocks[0];

                // make sure the preview is deleted
                previewRoadblocks[0].DeletePreview();
            }
            else
            {
                roadblockToSpawn = CreateInstance(RoadQuery.ToVehicleNode(LastDeterminedStreet ?? CalculateNewLocationForInstance()));
                lock (Instances)
                {
                    Instances.Add(roadblockToSpawn);
                }
            }

            Game.NewSafeFiber(() =>
            {
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
            }, "ManualPlacement.PlaceRoadblock");
        }

        /// <inheritdoc />
        public void RemoveRoadblocks(RemoveType removeType)
        {
            DoInternalInstanceRemoval(removeType);
        }

        #endregion

        #region Functions

        protected override ManualRoadblock CreateInstance(IVehicleNode street)
        {
            Assert.NotNull(street, "road cannot be null");
            if (street.GetType() == typeof(Intersection))
                return null;

            var roadblock = new ManualRoadblock(new ManualRoadblock.Request
            {
                Road = (Road)street,
                BarrierType = _barrier,
                VehicleType = _vehicleType,
                LightSourceType = _lightSourceType,
                PlacementType = _placementType,
                TargetHeading = Game.PlayerHeading,
                LimitSpeed = SpeedLimit,
                AddLights = LightSourceType != LightSourceType.None,
                CopsEnabled = CopsEnabled,
                Offset = Offset
            });
            Logger.Debug($"Created manual roadblock {roadblock}");
            return roadblock;
        }

        private void UpdateBarrier(BarrierModel newType)
        {
            _barrier = newType;
            DoInternalPreviewCreation(true);
        }

        private void UpdateVehicle(VehicleType value)
        {
            _vehicleType = value;
            DoInternalPreviewCreation(true);
        }

        private void UpdateLightSource(LightSourceType lightSourceType)
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

        #endregion
    }
}