using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Utils.Type;
using Rage;

namespace AutomaticRoadblocks.Utils.Road
{
    public class Road : IPreviewSupport
    {
        private const float LaneHeadingTolerance = 15f;

        internal Road()
        {
        }

        #region Properties

        /// <summary>
        /// Get the center position of the road.
        /// </summary>
        public Vector3 Position => Node.Position;

        /// <summary>
        /// Get the right side position of the road.
        /// </summary>
        public Vector3 RightSide { get; internal set; }

        /// <summary>
        /// Get the left side position of the road.
        /// </summary>
        public Vector3 LeftSide { get; internal set; }

        /// <summary>
        /// Get the lanes of this road.
        /// </summary>
        public IReadOnlyList<Lane> Lanes { get; internal set; }

        /// <summary>
        /// The vehicle node info on which this road is based.
        /// </summary>
        public NodeInfo Node { get; internal set; }

        /// <summary>
        /// Get the total number of lanes.
        /// </summary>
        public int NumberOfLanes1 { get; internal set; }

        /// <summary>
        /// Get the total number of lanes.
        /// </summary>
        public int NumberOfLanes2 { get; internal set; }

        /// <summary>
        /// Get the junction indicator.
        /// </summary>
        public int JunctionIndicator { get; internal set; }

        /// <summary>
        /// Get or set the width of the road.
        /// </summary>
        public float Width
        {
            get { return Lanes.Select(x => x.Width).Sum(); }
        }

        /// <summary>
        /// Check if the road position is at a junction.
        /// </summary>
        public bool IsAtJunction => JunctionIndicator != 0;

        /// <summary>
        /// Check if the road goes in one direction (no opposite lane present).
        /// </summary>
        public bool IsSingleDirection => IsSingleDirectionRoad();

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive { get; private set; }

        /// <inheritdoc />
        public void CreatePreview()
        {
            if (IsPreviewActive)
                return;

            DoInternalPreviewCreation();
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            if (!IsPreviewActive)
                return;

            IsPreviewActive = false;
            foreach (var lane in Lanes)
            {
                lane.DeletePreview();
            }

            Node.DeletePreview();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retrieve the lane which is the closest to the given position.
        /// </summary>
        /// <param name="position">The position to compare the lanes to.</param>
        /// <returns>Returns the lane closest to the given position.</returns>
        public Lane LaneClosestTo(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            var closestLaneDistance = 9999f;
            var closestLane = (Lane)null;

            foreach (var lane in Lanes)
            {
                var distance = position.DistanceTo(lane.Position);

                if (distance > closestLaneDistance)
                    continue;

                closestLaneDistance = distance;
                closestLane = lane;
            }

            return closestLane;
        }

        /// <summary>
        /// Retrieve the lanes which are heading towards the given heading.
        /// </summary>
        /// <param name="heading">The heading the lanes should follow.</param>
        /// <returns>Returns the lanes towards the same heading if any match, else an empty <see cref="IEnumerable{T}"/>.</returns>
        public IEnumerable<Lane> LanesHeadingTo(float heading)
        {
            return Lanes
                .Where(x => Math.Abs(x.Heading - heading) < LaneHeadingTolerance)
                .ToList();
        }

        public override string ToString()
        {
            return $"{nameof(Position)}: {Position}, {nameof(Width)}: {Width}, {nameof(RightSide)}: {RightSide}, {nameof(LeftSide)}: {LeftSide}\n" +
                   $"{nameof(NumberOfLanes1)}: {NumberOfLanes1}, {nameof(NumberOfLanes2)}: {NumberOfLanes2}, {nameof(JunctionIndicator)}: {JunctionIndicator}, " +
                   $"{nameof(IsAtJunction)}: {IsAtJunction}, {nameof(IsSingleDirection)}: {IsSingleDirection}\n" +
                   $"{nameof(Node)}: {Node}" +
                   "\n--- Lanes ---" +
                   $"\n{string.Join("\n", Lanes)}" +
                   "\n---";
        }

        protected bool Equals(Road other)
        {
            return Equals(Node, other.Node);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Road)obj);
        }

        public override int GetHashCode()
        {
            return (Node != null ? Node.GetHashCode() : 0);
        }

        #endregion

        #region Functions

        private bool IsSingleDirectionRoad()
        {
            return NumberOfLanes1 == 0 || NumberOfLanes2 == 0;
        }

        [Conditional("DEBUG")]
        private void DoInternalPreviewCreation()
        {
            IsPreviewActive = true;
            GameFiber.StartNew(() =>
            {
                var game = IoC.Instance.GetInstance<IGame>();

                while (IsPreviewActive)
                {
                    game.DrawSphere(Position, 0.5f, Color.White);
                    GameUtils.CreateMarker(LeftSide, EMarkerType.MarkerTypeVerticalCylinder, Color.Blue, 0.5f, 1.5f, false);
                    GameUtils.CreateMarker(RightSide, EMarkerType.MarkerTypeVerticalCylinder, Color.Green, 0.5f, 1.5f, false);
                    game.FiberYield();
                }
            });
            foreach (var lane in Lanes)
            {
                lane.CreatePreview();
            }

            Node.CreatePreview();
        }

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Defines the lane information within the road.
        /// </summary>
        public record Lane : IPreviewSupport
        {
            internal Lane()
            {
            }

            #region Properties

            /// <summary>
            /// Get the middle position of the lane.
            /// </summary>
            public Vector3 Position => CalculatePosition();

            /// <summary>
            /// Get the heading of the lane.
            /// </summary>
            public float Heading { get; internal set; }

            /// <summary>
            /// Get the right side start position of the lane.
            /// </summary>
            public Vector3 RightSide { get; internal set; }

            /// <summary>
            /// Get the left side start position of the lane.
            /// </summary>
            public Vector3 LeftSide { get; internal set; }

            /// <summary>
            /// Get the position of the node that was used for determining the heading of this lane.
            /// </summary>
            public Vector3 NodePosition { get; internal set; }

            /// <summary>
            /// Get the width of the lane.
            /// </summary>
            public float Width { get; internal set; }

            /// <summary>
            /// Verify if this lane heading is the opposite of the road it belongs to.
            /// </summary>
            public bool IsOppositeHeadingOfRoadNodeHeading { get; internal set; }

            #endregion

            #region IPreviewSupport

            /// <inheritdoc />
            public bool IsPreviewActive { get; private set; }

            /// <inheritdoc />
            public void CreatePreview()
            {
                if (IsPreviewActive)
                    return;

                DoInternalPreviewCreation();
            }

            /// <inheritdoc />
            public void DeletePreview()
            {
                if (!IsPreviewActive)
                    return;

                IsPreviewActive = false;
            }

            #endregion

            /// <summary>
            /// Create a new lane in the direction relative to this lane.
            /// This will create a clone of this lane (same <see cref="Width"/>, <see cref="Type"/>, etc.) which is relatively moved in regards to this lane. 
            /// </summary>
            /// <param name="direction">The direction in which the new lane should be moved.</param>
            /// <returns>Returns a cloned lane moved relative to this lane.</returns>
            public Lane MoveTo(Vector3 direction)
            {
                Assert.NotNull(direction, "direction cannot be null");
                return new Lane
                {
                    Heading = Heading,
                    RightSide = RightSide + direction,
                    LeftSide = LeftSide + direction,
                    NodePosition = NodePosition + direction,
                    Width = Width,
                    IsOppositeHeadingOfRoadNodeHeading = IsOppositeHeadingOfRoadNodeHeading
                };
            }

            public override string ToString()
            {
                return "{" +
                       $"{nameof(NodePosition)}: {NodePosition}, {nameof(Heading)}: {Heading}, {nameof(RightSide)}: {RightSide}, {nameof(LeftSide)}: {LeftSide}, " +
                       $"{nameof(Width)}: {Width}, {nameof(IsPreviewActive)}: {IsPreviewActive}" +
                       "}";
            }

            [Conditional("DEBUG")]
            private void DoInternalPreviewCreation()
            {
                var game = IoC.Instance.GetInstance<IGame>();

                IsPreviewActive = true;
                game.NewSafeFiber(() =>
                {
                    var rightSideDirection = MathHelper.ConvertHeadingToDirection(Heading) * 1f;
                    var leftSideDirection = MathHelper.ConvertHeadingToDirection(Heading) * 1.15f;

                    while (IsPreviewActive)
                    {
                        game.DrawArrow(FloatAboveGround(Position), rightSideDirection, Rotator.Zero, Width - 1f, Color.Red);
                        GameUtils.CreateMarker(RightSide + rightSideDirection, EMarkerType.MarkerTypeVerticalCylinder, Color.Yellow, 0.5f, 2f, false);
                        GameUtils.CreateMarker(LeftSide + leftSideDirection, EMarkerType.MarkerTypeVerticalCylinder, Color.DarkViolet, 0.5f, 2f, false);
                        game.FiberYield();
                    }
                }, "Road.Lane.CreatePreview");
            }

            private Vector3 CalculatePosition()
            {
                var moveHeading = MathHelper.NormalizeHeading(Heading + 90f);
                var moveDirection = MathHelper.ConvertHeadingToDirection(moveHeading);

                return RightSide + moveDirection * (Width / 2);
            }

            private static Vector3 FloatAboveGround(Vector3 position)
            {
                return position + Vector3.WorldUp * 0.25f;
            }
        }

        /// <summary>
        /// The vehicle node that was used to determine the road/lane.
        /// </summary>
        public class NodeInfo : IPreviewSupport
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
            public ENodeType Flags { get; internal set; }

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
}