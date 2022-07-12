using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Utils.Road
{
    public static class RoadUtils
    {
        #region Methods

        /// <summary>
        /// Get the closest road near the given position.
        /// </summary>
        /// <param name="position">Set the position to use as reference.</param>
        /// <param name="roadType">Set the road type.</param>
        /// <returns>Returns the position of the closest road.</returns>
        public static Road GetClosestRoad(Vector3 position, RoadType roadType)
        {
            Road closestRoad = null;
            var closestRoadDistance = 99999f;

            foreach (var road in GetNearbyRoads(position, roadType))
            {
                var roadDistanceToPosition = Vector3.Distance2D(road.Position, position);

                if (roadDistanceToPosition > closestRoadDistance)
                    continue;

                closestRoad = road;
                closestRoadDistance = roadDistanceToPosition;
            }

            return closestRoad;
        }

        /// <summary>
        /// Get the lane closest to the given point.
        /// </summary>
        /// <param name="road">Set the road to get the closest lane of.</param>
        /// <param name="closestToPoint">Set the point.</param>
        /// <returns>Returns the closest lane of the road in regards to the given point.</returns>
        public static Road.Lane GetClosestLane(Road road, Vector3 closestToPoint)
        {
            Assert.NotNull(road, "road cannot be null");
            Assert.NotNull(closestToPoint, "closestToPoint cannot be null");
            var distanceClosestLane = 9999f;
            Road.Lane closestLane = null;

            foreach (var lane in road.Lanes)
            {
                var laneDistance = Vector3.Distance2D(lane.RightSide, closestToPoint);

                if (laneDistance > distanceClosestLane)
                    continue;

                closestLane = lane;
                distanceClosestLane = laneDistance;
            }

            return closestLane;
        }

        /// <summary>
        /// Get nearby roads near the given position.
        /// </summary>
        /// <param name="position">Set the position to use as reference.</param>
        /// <param name="roadType">Set the road type.</param>
        /// <returns>Returns the position of the closest road.</returns>
        public static IEnumerable<Road> GetNearbyRoads(Vector3 position, RoadType roadType)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(roadType, "roadType cannot be null");

            NativeFunction.Natives.GET_CLOSEST_ROAD(position.X, position.Y, position.Z, 1f, 1, out Vector3 road1, out Vector3 road2, out int numberOfLanes1,
                out int numberOfLanes2, out float junctionIndication, (int)roadType);

            return new List<Road>
            {
                DiscoverRoad(road1, numberOfLanes1, numberOfLanes2, junctionIndication),
                DiscoverRoad(road2, numberOfLanes2, numberOfLanes1, junctionIndication)
            };
        }

        /// <summary>
        /// Get the opposite heading of the given heading.
        /// </summary>
        /// <param name="heading">Set the heading.</param>
        /// <returns>Returns the opposite heading of the given heading.</returns>
        public static float OppositeHeading(float heading)
        {
            return (heading + 180) % 360;
        }

        /// <summary>
        /// Check if the given lane is the left side lane of the road.
        /// </summary>
        /// <param name="road">The road to determine the lane side of.</param>
        /// <param name="lane">The lane on the road to verify.</param>
        /// <returns>Returns true if the given lane is the left lane of the road, else false.</returns>
        public static bool IsLeftSideLane(Road road, Road.Lane lane)
        {
            var distanceRightSide = Vector3.Distance2D(road.RightSide, lane.Position);
            var distanceLeftSide = Vector3.Distance2D(road.LeftSide, lane.Position);

            return distanceLeftSide < distanceRightSide;
        }

        /// <summary>
        /// Check if the road has multiple lanes in the same direction as the given lane.
        /// </summary>
        /// <param name="road">The road to verify the lanes of.</param>
        /// <param name="lane">The lane to check for same direction.</param>
        /// <returns>Returns true if the road has multiple lanes going in the same direction as the given lane.</returns>
        public static bool HasMultipleLanesInSameDirection(Road road, Road.Lane lane)
        {
            Assert.NotNull(road, "road cannot be null");
            return road.Lanes
                .Where(x => x != lane)
                .Any(x => Math.Abs(lane.Heading - x.Heading) < 1f);
        }

        /// <summary>
        /// Create a new speed zone.
        /// </summary>
        /// <param name="position">The position of the zone.</param>
        /// <param name="radius">The radius of the zone.</param>
        /// <param name="speed">The max speed within the zone.</param>
        /// <returns>Returns the created zone ID.</returns>
        public static int CreateSpeedZone(Vector3 position, float radius, float speed)
        {
            Assert.NotNull(position, "position cannot be null");
            return NativeFunction.CallByHash<int>(0x2CE544C68FB812A0, position.X, position.Y, position.Z, radius, speed, false);
        }

        /// <summary>
        /// Remove a speed zone.
        /// </summary>
        /// <param name="zoneId">The zone id to remove.</param>
        public static void RemoveSpeedZone(int zoneId)
        {
            Assert.NotNull(zoneId, "zoneId cannot be null");
            NativeFunction.CallByHash<int>(0x2CE544C68FB812A0, zoneId);
        }

        #endregion

        #region Functions

        private static Road DiscoverRoad(Vector3 roadPosition, int numberOfLanes1, int numberOfLanes2, float junctionIndication)
        {
            var vehicleNode = GetVehicleNode(roadPosition);
            var rightSideHeading = vehicleNode.Heading;
            var roadRightSide = GetLastPointOnTheLane(roadPosition, rightSideHeading - 90f);
            var roadLeftSide = GetLastPointOnTheLane(roadPosition, rightSideHeading + 90f);

            // Fix a side if it's the same as the middle of the road as GetLastPointOnTheLane probably failed to determine the last point
            if (roadRightSide == roadPosition)
                roadRightSide = FixFailedLastPointCalculation(roadPosition, roadLeftSide, rightSideHeading - 90f);
            if (roadLeftSide == roadPosition)
                roadLeftSide = FixFailedLastPointCalculation(roadPosition, roadRightSide, rightSideHeading + 90f);

            return RoadBuilder.Builder()
                .Position(roadPosition)
                .RightSide(roadRightSide)
                .LeftSide(roadLeftSide)
                .NumberOfLanes1(numberOfLanes1)
                .NumberOfLanes2(numberOfLanes2)
                .JunctionIndicator((int)junctionIndication)
                .Lanes(DiscoverLanes(roadRightSide, roadLeftSide, roadPosition, rightSideHeading, numberOfLanes1, numberOfLanes2))
                .Node(vehicleNode)
                .Build();
        }

        private static List<Road.Lane> DiscoverLanes(Vector3 roadRightSide, Vector3 roadLeftSide, Vector3 roadMiddle, float rightSideHeading,
            int numberOfLanes1, int numberOfLanes2)
        {
            var singleDirection = IsSingleDirectionRoad(numberOfLanes1, numberOfLanes2);
            var lanes = new List<Road.Lane>();

            if (singleDirection)
            {
                var numberOfLanes = numberOfLanes1 == 0 ? numberOfLanes2 : numberOfLanes1;
                var laneWidth = GetWidth(roadRightSide, roadLeftSide) / numberOfLanes;
                lanes.AddRange(CreateLanes(roadRightSide, rightSideHeading, numberOfLanes, laneWidth, false));
            }
            else
            {
                var laneWidthRight = GetWidth(roadRightSide, roadMiddle) / numberOfLanes1;
                var laneWidthLeft = GetWidth(roadMiddle, roadLeftSide) / numberOfLanes2;

                lanes.AddRange(CreateLanes(roadRightSide, rightSideHeading, numberOfLanes1, laneWidthRight, false));
                lanes.AddRange(CreateLanes(roadMiddle, rightSideHeading, numberOfLanes2, laneWidthLeft, true));
            }

            return lanes;
        }

        private static IEnumerable<Road.Lane> CreateLanes(Vector3 roadRightSide, float rightSideHeading, int numberOfLanes, float laneWidth,
            bool isOpposite)
        {
            var lastRightPosition = roadRightSide;
            var moveDirection = MathHelper.ConvertHeadingToDirection(rightSideHeading + 90f);
            var lanes = new List<Road.Lane>();

            for (var index = 1; index <= numberOfLanes; index++)
            {
                var laneLeftPosition = lastRightPosition + moveDirection * laneWidth;
                var vehicleNode = GetVehicleNode(lastRightPosition);
                var heading = isOpposite ? OppositeHeading(rightSideHeading) : rightSideHeading;
                lanes.Add(LaneBuilder.Builder()
                    .Number(index)
                    .Heading(heading)
                    .RightSide(isOpposite ? laneLeftPosition : lastRightPosition)
                    .LeftSide(isOpposite ? lastRightPosition : laneLeftPosition)
                    .NodePosition(vehicleNode.Position)
                    .Width(laneWidth)
                    .Build());
                lastRightPosition = laneLeftPosition;
            }

            return lanes;
        }

        // This fix is a simple workaround if the last point detection failed with the native function of GTA
        // It will assume that both sides have the same width and mirror the width to the other side to determine the point
        private static Vector3 FixFailedLastPointCalculation(Vector3 roadPosition, Vector3 otherSidePosition, float heading)
        {
            var widthOtherSide = Vector3.Distance2D(roadPosition, otherSidePosition);
            var directionOfTheSideToFix = MathHelper.ConvertHeadingToDirection(heading);
            return roadPosition + directionOfTheSideToFix * widthOtherSide;
        }

        private static Road.VehicleNode GetVehicleNode(Vector3 position)
        {
            NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING(position.X, position.Y, position.Z, out Vector3 nodePosition, out float nodeHeading,
                1, 3, 0);

            return new Road.VehicleNode
            {
                Position = nodePosition,
                Heading = MathHelper.NormalizeHeading(nodeHeading)
            };
        }

        private static Vector3 GetLastPointOnTheLane(Vector3 position, float heading)
        {
            var checkInterval = 2f;
            var lastPositionOnTheRoad = position;

            do
            {
                var lastPointResult = DetermineLastPointOnLane(lastPositionOnTheRoad, heading, checkInterval);
                lastPositionOnTheRoad = lastPointResult.LastPointOnRoad;
                checkInterval /= 2;
            } while (checkInterval > 0.1f);

            return lastPositionOnTheRoad;
        }

        private static LastPointResult DetermineLastPointOnLane(Vector3 position, float heading, float checkInterval)
        {
            var currentPosition = position;
            var direction = MathHelper.ConvertHeadingToDirection(heading);
            bool isPointOnRoad;
            Vector3 lastPositionOnTheRoad;

            do
            {
                lastPositionOnTheRoad = currentPosition;
                currentPosition += direction * checkInterval;
                isPointOnRoad = NativeFunction.Natives.IS_POINT_ON_ROAD<bool>(currentPosition.X, currentPosition.Y, currentPosition.Z);
            } while (isPointOnRoad);

            return new LastPointResult
            {
                LastCheckedPoint = currentPosition,
                LastPointOnRoad = lastPositionOnTheRoad
            };
        }

        private static float GetWidth(Vector3 point1, Vector3 point2)
        {
            return Vector3.Distance(point1, point2);
        }

        private static bool IsSingleDirectionRoad(int numberOfLanes1, int numberOfLanes2)
        {
            return numberOfLanes1 == 0 || numberOfLanes2 == 0;
        }

        #endregion
    }

    internal class LastPointResult
    {
        /// <summary>
        /// The last point that was checked and was not on the road anymore.
        /// </summary>
        public Vector3 LastCheckedPoint { get; set; }

        /// <summary>
        /// The last point that was check and was still on the road.
        /// </summary>
        public Vector3 LastPointOnRoad { get; set; }
    }

    internal class RoadBuilder
    {
        private readonly List<Road.Lane> _lanes = new();
        private Vector3 _position;
        private Vector3 _rightSide;
        private Vector3 _leftSide;
        private int _numberOfLanes1;
        private int _numberOfLanes2;
        private int _junctionIndicator;
        private Road.VehicleNode _node;

        private RoadBuilder()
        {
        }

        public static RoadBuilder Builder()
        {
            return new RoadBuilder();
        }

        public RoadBuilder Position(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _position = position;
            return this;
        }

        public RoadBuilder RightSide(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _rightSide = position;
            return this;
        }

        public RoadBuilder LeftSide(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _leftSide = position;
            return this;
        }

        public RoadBuilder NumberOfLanes1(int value)
        {
            _numberOfLanes1 = value;
            return this;
        }

        public RoadBuilder NumberOfLanes2(int value)
        {
            _numberOfLanes2 = value;
            return this;
        }

        public RoadBuilder JunctionIndicator(int value)
        {
            _junctionIndicator = value;
            return this;
        }

        public RoadBuilder Lanes(List<Road.Lane> lanes)
        {
            Assert.NotNull(lanes, "lanes cannot be null");
            _lanes.AddRange(lanes);
            return this;
        }

        public RoadBuilder Node(Road.VehicleNode node)
        {
            Assert.NotNull(node, "node cannot be null");
            _node = node;
            return this;
        }

        public Road Build()
        {
            Assert.NotNull(_position, "position has not been set");
            Assert.NotNull(_rightSide, "rightSide has not been set");
            Assert.NotNull(_leftSide, "leftSide has not been set");
            return new Road(_position, _rightSide, _leftSide, _lanes.AsReadOnly(), _node, _numberOfLanes1, _numberOfLanes2, _junctionIndicator);
        }
    }

    internal class LaneBuilder
    {
        private int _number;
        private float _heading;
        private Vector3 _rightSide;
        private Vector3 _leftSide;
        private Vector3 _nodePosition;
        private float _width;

        private LaneBuilder()
        {
        }

        public static LaneBuilder Builder()
        {
            return new LaneBuilder();
        }

        public LaneBuilder Number(int number)
        {
            _number = number;
            return this;
        }

        public LaneBuilder Heading(float heading)
        {
            Assert.NotNull(heading, "heading cannot be null");
            _heading = heading;
            return this;
        }

        public LaneBuilder RightSide(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _rightSide = position;
            return this;
        }

        public LaneBuilder LeftSide(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _leftSide = position;
            return this;
        }

        public LaneBuilder NodePosition(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _nodePosition = position;
            return this;
        }

        public LaneBuilder Width(float width)
        {
            Assert.NotNull(width, "width cannot be null");
            _width = width;
            return this;
        }

        public Road.Lane Build()
        {
            return new Road.Lane(_number, _heading, _rightSide, _leftSide, _nodePosition, _width);
        }
    }
}