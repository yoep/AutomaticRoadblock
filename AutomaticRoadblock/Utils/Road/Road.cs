using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.Preview;
using Rage;

namespace AutomaticRoadblocks.Utils.Road
{
    public class Road : IPreviewSupport
    {
        private const float LaneHeadingTolerance = 15f;

        #region Constructors

        internal Road(Vector3 position, Vector3 rightSide, Vector3 leftSide, IReadOnlyList<Lane> lanes, VehicleNode node, int numberOfLanes1,
            int numberOfLanes2, int junctionIndicator)
        {
            Position = position;
            RightSide = rightSide;
            LeftSide = leftSide;
            Lanes = lanes;
            Node = node;
            NumberOfLanes1 = numberOfLanes1;
            NumberOfLanes2 = numberOfLanes2;
            JunctionIndicator = junctionIndicator;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the center position of the road.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Get the right side position of the road.
        /// </summary>
        public Vector3 RightSide { get; }

        /// <summary>
        /// Get the left side position of the road.
        /// </summary>
        public Vector3 LeftSide { get; }

        /// <summary>
        /// Get the lanes of this road.
        /// </summary>
        public IReadOnlyList<Lane> Lanes { get; }

        /// <summary>
        /// Get the vehicle node that was used to determine this road.
        /// </summary>
        public VehicleNode Node { get; }

        /// <summary>
        /// Get the total number of lanes.
        /// </summary>
        public int NumberOfLanes1 { get; }

        /// <summary>
        /// Get the total number of lanes.
        /// </summary>
        public int NumberOfLanes2 { get; }

        /// <summary>
        /// Get the junction indicator.
        /// </summary>
        public int JunctionIndicator { get; }

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

        #region IPreviewSupport implementation

        /// <inheritdoc />
        public bool IsPreviewActive { get; private set; }

        /// <inheritdoc />
        public void CreatePreview()
        {
            if (IsPreviewActive)
                return;

            IsPreviewActive = true;
            GameFiber.StartNew(() =>
            {
                while (IsPreviewActive)
                {
                    Rage.Debug.DrawSphere(Position, 1f, Color.White);
                    Rage.Debug.DrawSphere(RightSide, 1f, Color.Blue);
                    Rage.Debug.DrawSphere(LeftSide, 1f, Color.Green);
                    GameFiber.Yield();
                }
            });
            foreach (var lane in Lanes)
            {
                lane.CreatePreview();
            }

            Node.CreatePreview();
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

        private bool Equals(Road other)
        {
            return Position.Equals(other.Position) && Equals(Node, other.Node);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Road)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Position.GetHashCode() * 397) ^ (Node != null ? Node.GetHashCode() : 0);
            }
        }

        #endregion

        #region Functions

        private bool IsSingleDirectionRoad()
        {
            return NumberOfLanes1 == 0 || NumberOfLanes2 == 0;
        }

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Defines the lane information within the road.
        /// </summary>
        public record Lane : IPreviewSupport
        {
            #region Constructors

            public Lane(int number, float heading, Vector3 rightSide, Vector3 leftSide, Vector3 nodePosition, float width, LaneType type)
            {
                Number = number;
                Heading = heading;
                RightSide = rightSide;
                LeftSide = leftSide;
                NodePosition = nodePosition;
                Width = width;
                Type = type;
                Position = CalculatePosition();
            }

            #endregion

            #region Properties

            /// <summary>
            /// Get the unique lane number.
            /// </summary>
            public int Number { get; }

            /// <summary>
            /// Get the heading of the lane.
            /// </summary>
            public float Heading { get; }

            /// <summary>
            /// Get the middle position of the lane.
            /// </summary>
            public Vector3 Position { get; }

            /// <summary>
            /// Get the right side start position of the lane.
            /// </summary>
            public Vector3 RightSide { get; }

            /// <summary>
            /// Get the left side start position of the lane.
            /// </summary>
            public Vector3 LeftSide { get; }

            /// <summary>
            /// Get the position of the node that was used for determining the heading of this lane.
            /// </summary>
            public Vector3 NodePosition { get; }

            /// <summary>
            /// Get the width of the lane.
            /// </summary>
            public float Width { get; }

            /// <summary>
            /// Get the lane type.
            /// </summary>
            public LaneType Type { get; }

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
                GameFiber.StartNew(() =>
                {
                    var direction = MathHelper.ConvertHeadingToDirection(Heading);
                    var rightSideStart = FloatAboveGround(RightSide + direction * (2f * Number));
                    var rightSideEnd = FloatAboveGround(RightSide + direction * (2f * Number + 2f));
                    var leftSideStart = FloatAboveGround(LeftSide + direction * (2f * Number));
                    var leftSideEnd = FloatAboveGround(LeftSide + direction * (2f * Number + 2f));

                    while (IsPreviewActive)
                    {
                        Rage.Debug.DrawArrow(FloatAboveGround(Position), direction, Rotator.Zero, Width, Color.Red);
                        Rage.Debug.DrawLine(rightSideStart, rightSideEnd, Color.Blue);
                        Rage.Debug.DrawLine(leftSideStart, leftSideEnd, Color.Green);
                        GameFiber.Yield();
                    }
                });
            }

            /// <inheritdoc />
            public void DeletePreview()
            {
                if (!IsPreviewActive)
                    return;

                IsPreviewActive = false;
            }

            #endregion

            public override string ToString()
            {
                return "{" +
                       $"{nameof(Number)}: {Number}, {nameof(Heading)}: {Heading}, {nameof(RightSide)}: {RightSide}, {nameof(LeftSide)}: {LeftSide}, " +
                       $"{nameof(NodePosition)}: {NodePosition}, {nameof(Width)}: {Width}, {nameof(Type)}: {Type}, {nameof(IsPreviewActive)}: {IsPreviewActive}" +
                       "}";
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

            public enum LaneType
            {
                LeftLane,
                MiddleLane,
                RightLane
            }
        }

        /// <summary>
        /// The vehicle node that was used to determine the road.
        /// </summary>
        public class VehicleNode : IPreviewSupport
        {
            /// <summary>
            /// The position of the vehicle node.
            /// </summary>
            public Vector3 Position { get; internal set; }

            /// <summary>
            /// The heading of the vehicle node.
            /// </summary>
            public float Heading { get; internal set; }

            #region IPreviewSupport

            /// <inheritdoc />
            public bool IsPreviewActive { get; private set; }

            /// <inheritdoc />
            public void CreatePreview()
            {
                if (IsPreviewActive)
                    return;

                var direction = MathHelper.ConvertHeadingToDirection(Heading);

                IsPreviewActive = true;
                GameFiber.StartNew(() =>
                {
                    while (IsPreviewActive)
                    {
                        Rage.Debug.DrawArrow(FloatAboveGround(Position), direction, Rotator.Zero, 3f, Color.Gold);
                        GameFiber.Yield();
                    }
                });
            }

            /// <inheritdoc />
            public void DeletePreview()
            {
                if (!IsPreviewActive)
                    return;

                IsPreviewActive = false;
            }

            #endregion

            public override string ToString()
            {
                return "{" + $"{nameof(Position)}: {Position}, {nameof(Heading)}: {Heading}" + "}";
            }

            protected bool Equals(VehicleNode other)
            {
                return Position.Equals(other.Position) && Heading.Equals(other.Heading);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((VehicleNode)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Position.GetHashCode() * 397) ^ Heading.GetHashCode();
                }
            }

            private static Vector3 FloatAboveGround(Vector3 position)
            {
                return position + Vector3.WorldUp * 1f;
            }
        }
    }
}