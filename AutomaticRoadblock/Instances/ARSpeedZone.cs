using System;
using System.Diagnostics;
using System.Drawing;
using AutomaticRoadblocks.Logging;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Type;
using Rage;

namespace AutomaticRoadblocks.Instances
{
    public class ARSpeedZone : IPreviewSupport, IDisposable
    {
        private readonly ILogger _logger = IoC.Instance.GetInstance<ILogger>();

        private uint? _speedZoneId;

        public ARSpeedZone(Vector3 position, float radius, float speedLimit)
        {
            Position = position;
            Radius = radius;
            SpeedLimit = speedLimit;
        }

        #region Properties

        /// <summary>
        /// The location of the speed zone.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// The radius of the speed zone.
        /// </summary>
        public float Radius { get; }

        /// <summary>
        /// The speed limit applied within the speed zone.
        /// </summary>
        public float SpeedLimit { get; }

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive { get; private set; }

        /// <inheritdoc />
        public void CreatePreview()
        {
            if (IsPreviewActive)
                return;

            IsPreviewActive = true;
            DrawDebugInfo();
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            IsPreviewActive = false;
        }

        #endregion

        #region Methods

        public void Spawn()
        {
            // verify if a speed zone was already created
            // if so, ignore this action
            if (_speedZoneId != 0)
                return;

            _logger.Trace($"Creating speed zone limit at roadblock location {Position}");
            try
            {
                _speedZoneId = World.AddSpeedZone(Position, Radius, SpeedLimit);
                _logger.Debug($"Created speed zone at {Position} with ID {_speedZoneId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create roadblock speed zone at {Position}, {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_speedZoneId.HasValue)
                return;

            if (World.RemoveSpeedZone(_speedZoneId.Value))
            {
                _logger.Debug($"Removed speed zone at {Position} with ID {_speedZoneId}");
            }
            else
            {
                _logger.Warn($"Failed to remove speed zone at {Position} with ID {_speedZoneId}");
            }
        }

        #endregion

        #region Functions

        [Conditional("DEBUG")]
        private void DrawDebugInfo()
        {
            GameUtils.NewSafeFiber(() =>
            {
                var mainColor = Color.Lavender;
                var color = Color.FromArgb(50, mainColor.R, mainColor.G, mainColor.B);

                while (IsPreviewActive)
                {
                    GameFiber.Yield();
                    GameUtils.CreateMarker(Position, EMarkerType.MarkerTypeVerticalCylinder, color, Radius, 2f, false);
                }
            }, "ARSpeedZone.DrawDebugInfo");
        }

        #endregion
    }
}