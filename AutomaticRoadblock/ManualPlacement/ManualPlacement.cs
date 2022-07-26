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
        public bool FlaresEnabled { get; set; }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _roadblocks.ForEach(x => x.Dispose());
            _roadblocks.Clear();
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
        public void RemovePreview()
        {
            _lastDeterminedRoad = null;
            var roadblocksToClean = _roadblocks
                .Where(x => x.IsPreviewActive)
                .ToList();

            if (roadblocksToClean.Count == 0)
                return;

            _logger.Debug($"Cleaning a total of {roadblocksToClean.Count} manual roadblock previews");
            roadblocksToClean.ForEach(x =>
            {
                _logger.Trace($"Removing manual roadblock preview {x}");
                x.DeletePreview();
                _roadblocks.Remove(x);
            });
        }

        /// <inheritdoc />
        public void PlaceRoadblock()
        {
        }

        #endregion

        #region Functions

        private void CreateManualRoadblockPreview(Road road, bool force)
        {
            if (!force && Equals(road, _lastDeterminedRoad))
                return;

            // remove any existing previews first
            RemovePreview();

            var roadblock = new ManualRoadblock(road, Barrier, VehicleType, _game.PlayerHeading, false, false);
            roadblock.CreatePreview();

            _roadblocks.Add(roadblock);
            _lastDeterminedRoad = road;
        }

        private static void CreatePreviewMarker(Road road)
        {
            GameUtils.CreateMarker(road.Position, MarkerType.MarkerTypeVerticalCylinder, Color.LightBlue, 2.5f, false);
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
        
        #endregion
    }
}