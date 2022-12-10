using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Street.Info
{
    public class Intersection : IVehicleNode
    {
        #region Properties

        /// <inheritdoc />
        public Vector3 Position { get; internal set; }

        /// <inheritdoc />
        public float Heading { get; internal set; }

        /// <inheritdoc />
        public EStreetType Type => EStreetType.Intersection;

        /// <summary>
        /// The directions of the intersection.
        /// </summary>
        internal List<VehicleNodeInfo> Directions { get; set; }

        /// <summary>
        /// The roads which connect to this intersection.
        /// </summary>
        internal List<Road> Roads { get; set; }

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive { get; private set; }

        /// <inheritdoc />
        public void CreatePreview()
        {
            DoInternalPreviewCreation();
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            if (!IsPreviewActive)
                return;

            IsPreviewActive = false;
            Roads?.ForEach(x => x.DeletePreview());
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{nameof(Position)}: {Position}, {nameof(Heading)}: {Heading}, {nameof(Type)}: {Type}, number of {nameof(Directions)}: {Directions.Count}, " +
                $"number of {nameof(Roads)}: {Roads.Count}\n" +
                $"--- {nameof(Roads)} ---\n" +
                $"{string.Join("\n", Roads)}";
        }

        #endregion

        #region Functions

        [Conditional("DEBUG")]
        private void DoInternalPreviewCreation()
        {
            if (IsPreviewActive)
                return;

            IsPreviewActive = true;
            Roads?.ForEach(x => x.CreatePreview());
            GameUtils.NewSafeFiber(() =>
            {
                var mainArrowPosition = Position + Vector3.WorldUp * 1.5f;
                var mainArrowDirection = MathHelper.ConvertHeadingToDirection(Heading);

                while (IsPreviewActive)
                {
                    GameUtils.DrawSphere(Position, 0.5f, Color.DeepPink);
                    GameUtils.DrawArrow(mainArrowPosition, mainArrowDirection, Rotator.Zero, 0.5f, Color.DeepPink);
                    Directions?.ForEach(x => DrawDirection(x.Position, x.Heading));
                    GameFiber.Yield();
                }
            }, "Intersection.Preview");
        }

        private static void DrawDirection(Vector3 position, float heading)
        {
            var direction = MathHelper.ConvertHeadingToDirection(heading);
            var drawPosition = position
                               + Vector3.WorldUp * 1.5f
                               + direction * 0.6f;
            GameUtils.DrawArrow(drawPosition, direction, Rotator.Zero, 0.5f, Color.White);
        }

        #endregion
    }
}