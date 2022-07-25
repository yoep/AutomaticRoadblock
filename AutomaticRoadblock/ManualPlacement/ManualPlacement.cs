using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Utils.Type;
using Rage;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class ManualPlacement : IManualPlacement
    {
        private readonly IGame _game;
        private readonly ISettingsManager _settingsManager;
        private readonly List<ManualRoadblock> _roadblocks = new();

        private Road _lastDeterminedRoad;

        public ManualPlacement(IGame game, ISettingsManager settingsManager)
        {
            _game = game;
            _settingsManager = settingsManager;
        }

        #region Methods

        /// <inheritdoc />
        public Road DetermineLocation()
        {
            var position = _game.PlayerPosition;
            var renderDirection = MathHelper.ConvertHeadingToDirection(_game.PlayerHeading);

            return RoadUtils.FindClosestRoad(position + renderDirection * 5f, RoadType.All);
        }

        /// <inheritdoc />
        public void CreatePreview()
        {
            var road = DetermineLocation();

            if (_settingsManager.ManualPlacementSettings.EnablePreview)
            {
                CreateManualRoadblockPreview(road);
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
            _roadblocks
                .Where(x => x.IsPreviewActive)
                .ToList()
                .ForEach(x => x.DeletePreview());
        }

        /// <inheritdoc />
        public void PlaceRoadblock()
        {
            
        }

        #endregion

        private void CreateManualRoadblockPreview(Road road)
        {
            if (road == _lastDeterminedRoad)
                return;

            // remove any existing previews first
            RemovePreview();

            _roadblocks.Add(new ManualRoadblock(road, BarrierType.SmallCone));
            _lastDeterminedRoad = road;
        }

        private static void CreatePreviewMarker(Road road)
        {
            GameUtils.CreateMarker(road.Position, MarkerType.MarkerTypeVerticalCylinder, Color.LightBlue, 2.5f, false);
        }
    }
}