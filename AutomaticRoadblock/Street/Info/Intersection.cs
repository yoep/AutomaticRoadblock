using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using AutomaticRoadblocks.AbstractionLayer;
using Rage;

namespace AutomaticRoadblocks.Street.Info
{
    public class Intersection : IVehicleNode
    {
        private static readonly IGame Game = IoC.Instance.GetInstance<IGame>();

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
            DoInternalPReviewCreation();
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
            return $"{nameof(Position)}: {Position}, {nameof(Heading)}: {Heading}, {nameof(Type)}: {Type}, number of {nameof(Directions)}: {Directions.Count}," +
                   $"number of {nameof(Roads)}: {Roads.Count}";
        }

        #endregion

        #region Functions

        [Conditional("DEBUG")]
        private void DoInternalPReviewCreation()
        {
            if (IsPreviewActive)
                return;

            IsPreviewActive = true;
            Roads?.ForEach(x => x.CreatePreview());
            Game.NewSafeFiber(() =>
            {
                while (IsPreviewActive)
                {
                    Game.DrawSphere(Position, 0.5f, Color.DeepPink);
                    DrawDirection(Position, Heading);
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
                               + direction * 0.4f;
            Game.DrawArrow(drawPosition, direction, Rotator.Zero, 0.5f, Color.White);
        }

        #endregion
    }
}