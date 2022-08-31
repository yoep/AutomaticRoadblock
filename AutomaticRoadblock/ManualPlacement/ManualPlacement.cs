using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Vehicles;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class ManualPlacement : AbstractInstancePlacementManager<ManualRoadblock>, IManualPlacement
    {
        private readonly ISettingsManager _settingsManager;

        private BarrierType _barrier = BarrierType.SmallCone;
        private VehicleType _vehicleType = VehicleType.Locale;
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
        public BarrierType Barrier
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
        public float Offset { get => _offset; set => UpdateOffset(value); }

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
                roadblockToSpawn = CreateInstance(LastDeterminedRoad ?? CalculateNewLocationForInstance());
                Logger.Trace("Created the manual roadblock");
                lock (Instances)
                {
                    Instances.Add(roadblockToSpawn);
                }
            }

            Game.NewSafeFiber(() => { roadblockToSpawn.Spawn(); }, "ManualPlacement.PlaceRoadblock");
        }

        /// <inheritdoc />
        public void RemoveRoadblocks(RemoveType removeType)
        {
            DoInternalInstanceRemoval(removeType);
        }

        #endregion

        #region Functions

        protected override ManualRoadblock CreateInstance(Road road)
        {
            Assert.NotNull(road, "road cannot be null");
            var roadblock = new ManualRoadblock(new ManualRoadblock.Request
            {
                Road = road,
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
            Logger.Trace($"Created manual roadblock {roadblock}");
            return roadblock;
        }

        private void UpdateBarrier(BarrierType newType)
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