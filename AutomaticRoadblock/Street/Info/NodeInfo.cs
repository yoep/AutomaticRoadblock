using System.Drawing;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Preview;
using Rage;

namespace AutomaticRoadblocks.Street.Info
{
    internal class NodeInfo : IPreviewSupport
        {
            internal NodeInfo(Vector3 position, float heading, int numberOfLanes1, int numberOfLanes2, float atJunction)
            {
                Position = position;
                Heading = heading;
                NumberOfLanes1 = numberOfLanes1;
                NumberOfLanes2 = numberOfLanes2;
                AtJunction = atJunction;
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
            /// The right side number of lanes for the node.
            /// </summary>
            public int NumberOfLanes1 { get; }

            /// <summary>
            /// The left side number of lanes for the node.
            /// </summary>
            public int NumberOfLanes2 { get; }

            /// <summary>
            /// Indicates if this node is at a junction.
            /// </summary>
            public float AtJunction { get; }

            /// <summary>
            /// The traffic density of the node.
            /// </summary>
            public int Density { get; internal set; }

            /// <summary>
            /// The flags of the node.
            /// </summary>
            public ENodeFlag Flags { get; internal set; }

            #endregion

            #region Methods

            public override string ToString()
            {
                return $"{nameof(Position)}: {Position}, {nameof(Heading)}: {Heading}, {nameof(NumberOfLanes1)}: {NumberOfLanes1}, " +
                       $"{nameof(NumberOfLanes2)}: {NumberOfLanes2}, {nameof(AtJunction)}: {AtJunction}, {nameof(Density)}: {Density}, {nameof(Flags)}: {Flags} ({(int)Flags})";
            }

            protected bool Equals(NodeInfo other)
            {
                return Position.Equals(other.Position) && Heading.Equals(other.Heading);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((NodeInfo)obj);
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