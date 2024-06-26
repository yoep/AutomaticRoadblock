using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Type;
using Rage;

namespace AutomaticRoadblocks.Street.Info
{
    public class Road : IVehicleNode
    {
        private const float LaneHeadingTolerance = 20f;

        internal Road()
        {
        }

        #region Properties

        /// <inheritdoc />
        public Vector3 Position => Node.Position;

        /// <inheritdoc />
        public float Heading => Node.Heading;

        /// <inheritdoc />
        public EStreetType Type => EStreetType.Road;

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
        /// The lanes going in the same direction as the road.
        /// </summary>
        public IReadOnlyList<Lane> LanesSameDirection => Lanes.Where(x => !x.IsOppositeHeadingOfRoadNodeHeading).ToList();

        /// <summary>
        /// The lanes going in the opposite direction of the road.
        /// </summary>
        public IReadOnlyList<Lane> LanesOppositeDirection => Lanes.Where(x => x.IsOppositeHeadingOfRoadNodeHeading).ToList();

        /// <summary>
        /// Get the total number of lanes.
        /// </summary>
        public int NumberOfLanesSameDirection => LanesSameDirection.Count;

        /// <summary>
        /// Get the total number of lanes.
        /// </summary>
        public int NumberOfLanesOppositeDirection => LanesOppositeDirection.Count;

        /// <summary>
        /// Get or set the width of the road.
        /// </summary>
        public float Width
        {
            get { return Lanes.Select(x => x.Width).Sum(); }
        }

        /// <summary>
        /// Check if the road goes in one direction (no opposite lane present).
        /// </summary>
        public bool IsSingleDirection => IsSingleDirectionRoad();

        /// <summary>
        /// The vehicle node info on which this road is based.
        /// </summary>
        internal VehicleNodeInfo Node { get; set; }

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
            var normalizedHeading = MathHelper.NormalizeHeading(heading);
            return Lanes
                .Where(x => MathHelper.NormalizeHeading(x.Heading - normalizedHeading) <= LaneHeadingTolerance)
                .ToList();
        }

        public override string ToString()
        {
            return $"{nameof(Position)}: {Position}, {nameof(Width)}: {Width}, {nameof(RightSide)}: {RightSide}, {nameof(LeftSide)}: {LeftSide}\n" +
                   $"{nameof(NumberOfLanesSameDirection)}: {NumberOfLanesSameDirection}, {nameof(NumberOfLanesOppositeDirection)}: {NumberOfLanesOppositeDirection}, " +
                   $"{nameof(IsSingleDirection)}: {IsSingleDirection}, {nameof(Node)}: {Node}" +
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
            return NumberOfLanesSameDirection == 0 || NumberOfLanesOppositeDirection == 0;
        }

        [Conditional("DEBUG")]
        private void DoInternalPreviewCreation()
        {
            IsPreviewActive = true;
            GameFiber.StartNew(() =>
            {
                var color = Color.LightGray;

                if ((Node.Flags & (ENodeFlag.IsGravelRoad | ENodeFlag.IsOffRoad)) != 0)
                {
                    color = Color.SandyBrown;
                }
                else if (Node.Flags.HasFlag(ENodeFlag.IsBackroad))
                {
                    color = Color.DarkSlateGray;
                }

                while (IsPreviewActive)
                {
                    GameUtils.DrawSphere(Position, 0.5f, color);
                    GameUtils.CreateMarker(LeftSide, EMarkerType.MarkerTypeVerticalCylinder, Color.Blue, 0.5f, 1.5f, false);
                    GameUtils.CreateMarker(RightSide, EMarkerType.MarkerTypeVerticalCylinder, Color.Green, 0.5f, 1.5f, false);
                    GameFiber.Yield();
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
                IsPreviewActive = true;
                GameUtils.NewSafeFiber(() =>
                {
                    var rightSideDirection = MathHelper.ConvertHeadingToDirection(Heading) * 1f;
                    var leftSideDirection = MathHelper.ConvertHeadingToDirection(Heading) * 1.25f;
                    var colorLaneArrow = Color.FromArgb(125, Color.Red);

                    while (IsPreviewActive)
                    {
                        GameUtils.DrawArrow(FloatAboveGround(Position), rightSideDirection, Rotator.Zero, Width - 1f, colorLaneArrow);
                        GameUtils.CreateMarker(RightSide + rightSideDirection, EMarkerType.MarkerTypeVerticalCylinder, Color.Yellow, 0.5f, 2f, false);
                        GameUtils.CreateMarker(LeftSide + leftSideDirection, EMarkerType.MarkerTypeVerticalCylinder, Color.DarkViolet, 0.5f, 2f, false);
                        GameFiber.Yield();
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
    }
}