using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Utils.Type;
using Rage;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class ManualPlacement : IManualPlacement
    {
        private readonly ILogger _logger;
        private readonly IGame _game;
        private readonly ISettingsManager _settingsManager;
        private readonly List<ManualRoadblock> _roadblocks = new();

        private Road _lastDeterminedRoad;
        private BarrierType _barrier = BarrierType.SmallCone;
        private VehicleType _vehicleType = VehicleType.Locale;
        private LightSourceType _lightSourceType = LightSourceType.Flares;

        public ManualPlacement(ILogger logger, IGame game, ISettingsManager settingsManager)
        {
            _game = game;
            _settingsManager = settingsManager;
            _logger = logger;
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
        public bool FlaresEnabled { get; set; }

        /// <summary>
        /// Get a list of roadblocks which are previewed.
        /// <remarks>Make sure this property is called in a lock statement.</remarks>
        /// </summary>
        private List<ManualRoadblock> PreviewRoadblocks => _roadblocks
            .Where(x => x.IsPreviewActive)
            .ToList();

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            lock (_roadblocks)
            {
                _roadblocks.ForEach(x => x.Dispose());
                _roadblocks.Clear();
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Road DetermineLocation()
        {
            var position = _game.PlayerPosition;
            var renderDirection = MathHelper.ConvertHeadingToDirection(_game.PlayerHeading);

            return RoadUtils.FindClosestRoad(position + renderDirection * 5f, RoadType.All);
        }

        /// <inheritdoc />
        public void CreatePreview(bool force = false)
        {
            var road = DetermineLocation();

            if (_settingsManager.ManualPlacementSettings.EnablePreview)
            {
                CreateManualRoadblockPreview(road, force);
            }
            else
            {
                CreatePreviewMarker(road);
            }
        }

        /// <inheritdoc />
        public void RemovePreviews()
        {
            DoRemovePreviews();
        }

        /// <inheritdoc />
        public void PlaceRoadblock()
        {
            ManualRoadblock roadblockToSpawn;
            List<ManualRoadblock> previewRoadblocks;

            lock (_roadblocks)
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
                roadblockToSpawn = CreateRoadblock(_lastDeterminedRoad);
                _logger.Trace("Created the manual roadblock");
                lock (_roadblocks)
                {
                    _roadblocks.Add(roadblockToSpawn);
                }
            }

            _game.NewSafeFiber(() => { roadblockToSpawn.Spawn(); }, "ManualPlacement.PlaceRoadblock");
        }

        #endregion

        #region Functions

        private void CreateManualRoadblockPreview(Road road, bool force)
        {
            if (!force && Equals(road, _lastDeterminedRoad))
                return;
            
            // remove any existing previews first
            RemovePreviews();
            
            _lastDeterminedRoad = road;
            _game.NewSafeFiber(() =>
            {
                lock (_roadblocks)
                {
                    var roadblock = CreateRoadblock(road);
                    roadblock.CreatePreview();

                    _roadblocks.Add(roadblock);
                }
            }, "ManualPlacement.CreateManualRoadblockPreview");
        }

        private ManualRoadblock CreateRoadblock(Road road)
        {
            Assert.NotNull(road, "road cannot be null");
            return new ManualRoadblock(road, Barrier, VehicleType, LightSourceType, _game.PlayerHeading, false,
                LightSourceType != LightSourceType.None);
        }

        private void DoRemovePreviews()
        {
            if (_lastDeterminedRoad == null)
                return;

            lock (_roadblocks)
            {
                _lastDeterminedRoad = null;
                var roadblocksToClean = PreviewRoadblocks;

                if (roadblocksToClean.Count == 0)
                    return;

                _logger.Debug($"Cleaning a total of {roadblocksToClean.Count} manual roadblock previews");
                roadblocksToClean.ForEach(x =>
                {
                    _logger.Trace($"Removing manual roadblock preview {x}");
                    x.DeletePreview();

                    if (_roadblocks.Remove(x))
                    {
                        _logger.Warn($"Failed to remove roadblock {x} from the known roadblocks");
                    }
                });
            }
        }

        private void UpdateBarrier(BarrierType newType)
        {
            _barrier = newType;
            CreatePreview(true);
        }

        private void UpdateVehicle(VehicleType value)
        {
            _vehicleType = value;
            CreatePreview(true);
        }

        private void UpdateLightSource(LightSourceType lightSourceType)
        {
            _lightSourceType = lightSourceType;
            CreatePreview(true);
        }

        private static void CreatePreviewMarker(Road road)
        {
            GameUtils.CreateMarker(road.Position, MarkerType.MarkerTypeVerticalCylinder, Color.White, 2.5f, false);
        }

        #endregion
    }
}