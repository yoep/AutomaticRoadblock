using System.Drawing;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Preview;
using Rage;

namespace AutomaticRoadblocks.Street.Info
{
    public class VehicleNodeInfo : IPreviewSupport
    {
        internal VehicleNodeInfo(Vector3 position, float heading)
        {
            Position = position;
            Heading = heading;
        }

        #region Properties

        /// <summary>
        /// The position of the node.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// The heading of the node.
        /// </summary>
        public float Heading { get; }

        /// <summary>
        /// The number of lanes in the same direction as the node.
        /// </summary>
        public int LanesInSameDirection { get; internal set; }

        /// <summary>
        /// The number of lanes in the opposite direction as the node.
        /// </summary>
        public int LanesInOppositeDirection { get; internal set; }

        /// <summary>
        /// The traffic density of the node.
        /// </summary>
        public int Density { get; internal set; }

        /// <summary>
        /// The flags of the node.
        /// </summary>
        public ENodeFlag Flags { get; internal set; }

        /// <summary>
        /// Verify if this node is a junction node.
        /// </summary>
        public bool IsJunctionNode => Flags.HasFlag(ENodeFlag.IsJunction);

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"{nameof(Position)}: {Position}, {nameof(Heading)}: {Heading}, {nameof(LanesInSameDirection)}: {LanesInSameDirection}, " +
                   $"{nameof(LanesInOppositeDirection)}: {LanesInOppositeDirection}, {nameof(Density)}: {Density}, {nameof(Flags)}: {Flags} ({(int)Flags})";
        }

        protected bool Equals(VehicleNodeInfo other)
        {
            return Position.Equals(other.Position) && Heading.Equals(other.Heading);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VehicleNodeInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Position.GetHashCode() * 397) ^ Heading.GetHashCode();
            }
        }

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive { get; private set; }

        /// <inheritdoc />
        public void CreatePreview()
        {
            if (IsPreviewActive)
                return;

            var game = IoC.Instance.GetInstance<IGame>();
            var direction = MathHelper.ConvertHeadingToDirection(Heading);

            IsPreviewActive = true;
            game.NewSafeFiber(() =>
            {
                while (IsPreviewActive)
                {
                    game.DrawArrow(FloatAboveGround(Position), direction, Rotator.Zero, 3f, Color.Gold);
                    game.FiberYield();
                }
            }, "Road.VehicleNode.CreatePreview");
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            if (!IsPreviewActive)
                return;

            IsPreviewActive = false;
        }

        #endregion

        #region Functions

        private static Vector3 FloatAboveGround(Vector3 position)
        {
            return position + Vector3.WorldUp * 1f;
        }

        #endregion
    }
}