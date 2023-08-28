using System;
using System.Diagnostics;
using System.Drawing;
using AutomaticRoadblocks.Logging;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Utils;
using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Instances
{
    /// <summary>
    /// Closes one or more vehicle nodes within the given area.
    /// </summary>
    public class ARCloseNodes : IPreviewSupport, IDisposable
    {
        private readonly ILogger _logger = IoC.Instance.GetInstance<ILogger>();

        public ARCloseNodes(Vector3 topLeftPosition, Vector3 bottomRightPosition)
        {
            TopLeftPosition = topLeftPosition + Vector3.WorldUp * 5f;
            BottomRightPosition = bottomRightPosition + Vector3.WorldDown * 5f;
        }

        #region Properties

        /// <summary>
        /// The top left position of the area to close.
        /// </summary>
        public Vector3 TopLeftPosition { get; }


        /// <summary>
        /// The bottom right position of the area to close.
        /// </summary>
        public Vector3 BottomRightPosition { get; }

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
            SetRoadsWithinArea(false);
            _logger.Info($"Disabled vehicle nodes within {TopLeftPosition}x{BottomRightPosition}");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            DeletePreview();
            SetRoadsWithinArea(true);
            _logger.Info($"Enabled vehicles nodes within {TopLeftPosition}x{BottomRightPosition}");
        }

        #endregion

        #region Functions

        private void SetRoadsWithinArea(bool nodesEnabled)
        {
            // PATHFIND::SET_ROADS_IN_AREA
            NativeFunction.CallByHash<int>(0xBF1A602B5BA52FEE, TopLeftPosition.X, TopLeftPosition.Y, TopLeftPosition.Z, BottomRightPosition.X,
                BottomRightPosition.Y, BottomRightPosition.Z, nodesEnabled, false);
        }

        [Conditional("DEBUG")]
        private void DrawDebugInfo()
        {
            GameUtils.NewSafeFiber(() =>
            {
                var mainColor = Color.DarkOrange;
                var color = Color.FromArgb(75, mainColor.R, mainColor.G, mainColor.B);
                var middlePosition = (TopLeftPosition + BottomRightPosition) * 0.5f;
                var renderPosition1 = new Vector3(TopLeftPosition.X, TopLeftPosition.Y, TopLeftPosition.Z);
                var renderPosition2 = new Vector3(BottomRightPosition.X, BottomRightPosition.Y, TopLeftPosition.Z);
                var flatTopLeft = new Vector3(TopLeftPosition.X, TopLeftPosition.Y, 0f);
                var flatBottomRight = new Vector3(BottomRightPosition.X, BottomRightPosition.Y, 0f);
                var orientation = (flatTopLeft - flatBottomRight).ToQuaternion();

                while (IsPreviewActive)
                {
                    GameFiber.Yield();
                    Rage.Debug.DrawWireBox(middlePosition, orientation, TopLeftPosition - BottomRightPosition, mainColor);
                    Rage.Debug.DrawSphere(middlePosition, 1f, color);
                    Rage.Debug.DrawSphere(renderPosition1, 0.5f, color);
                    Rage.Debug.DrawSphere(renderPosition2, 0.5f, color);
                }
            }, $"{GetType()}.CreatePreview");
        }

        #endregion
    }
}