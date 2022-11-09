using System.Collections.Generic;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Street.Factory
{
    internal static class RoadFactory
    {
        private const float MinLaneDistance = 3f;
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        internal static Road Create(VehicleNodeInfo nodeInfo)
        {
            CalculateRoadSidePoints(nodeInfo, out var roadRightSide, out var roadLeftSide);

            return new Road
            {
                RightSide = roadRightSide,
                LeftSide = roadLeftSide,
                Lanes = DiscoverLanes(roadRightSide, roadLeftSide, nodeInfo.Position, nodeInfo.Heading, nodeInfo.LanesInSameDirection,
                    nodeInfo.LanesInOppositeDirection),
                Node = nodeInfo,
            };
        }

        private static void CalculateRoadSidePoints(VehicleNodeInfo nodeInfo, out Vector3 rightSide, out Vector3 leftSide)
        {
            var nodeHeading = nodeInfo.Heading;
            var rightSideHeading = nodeHeading - 90f;
            var leftSideHeading = nodeHeading + 90f;

            rightSide = LastPointOnRoadStrategy.CalculateLastPointOnRoad(nodeInfo.Position, rightSideHeading);
            leftSide = LastPointOnRoadStrategy.CalculateLastPointOnRoad(nodeInfo.Position, leftSideHeading);

            // verify if the points are not the same
            // if so, correct the left side
            if (rightSide.Equals(leftSide))
            {
                leftSide = nodeInfo.Position + MathHelper.ConvertHeadingToDirection(leftSideHeading) * nodeInfo.Position.DistanceTo2D(rightSide);
            }

            // verify if the lane was calculated correctly
            if (nodeInfo.Position.DistanceTo2D(rightSide) < MinLaneDistance)
            {
                var numberOfLanes = nodeInfo.LanesInSameDirection == 0 ? 1 : nodeInfo.LanesInSameDirection;
                rightSide = nodeInfo.Position + MathHelper.ConvertHeadingToDirection(rightSideHeading) * (MinLaneDistance * numberOfLanes);
                Logger.Warn("Used default lane width for right side as road width calculation failed");
            }

            if (nodeInfo.Position.DistanceTo2D(leftSide) < MinLaneDistance)
            {
                var numberOfLanes = nodeInfo.LanesInOppositeDirection == 0 ? 1 : nodeInfo.LanesInOppositeDirection;
                leftSide = nodeInfo.Position + MathHelper.ConvertHeadingToDirection(leftSideHeading) * (MinLaneDistance * numberOfLanes);
                Logger.Warn("Used default lane width for left side as road width calculation failed");
            }
        }

        private static List<Road.Lane> DiscoverLanes(Vector3 roadRightSide, Vector3 roadLeftSide, Vector3 roadMiddle, float rightSideHeading,
            int numberOfLanes1, int numberOfLanes2)
        {
            var lanes = new List<Road.Lane>();

            // verify if there is currently only one lane
            // then calculate the lane from right to left
            if (numberOfLanes1 == 0 || numberOfLanes2 == 0)
            {
                var numberOfLanes = numberOfLanes1 == 0 ? numberOfLanes2 : numberOfLanes1;
                lanes.AddRange(CreateLanes(roadRightSide, roadLeftSide, rightSideHeading, numberOfLanes, false));
            }
            else
            {
                lanes.AddRange(CreateLanes(roadRightSide, roadMiddle, rightSideHeading, numberOfLanes1, false));
                lanes.AddRange(CreateLanes(roadLeftSide, roadMiddle, MathHelper.NormalizeHeading(rightSideHeading - 180), numberOfLanes2, true));
            }

            return lanes;
        }

        private static IEnumerable<Road.Lane> CreateLanes(Vector3 rightSidePosition, Vector3 leftSidePosition, float laneHeading, int numberOfLanes,
            bool isOpposite)
        {
            var lastRightPosition = rightSidePosition;
            var laneWidth = rightSidePosition.DistanceTo(leftSidePosition) / numberOfLanes;
            var moveDirection = MathHelper.ConvertHeadingToDirection(laneHeading + 90f);
            var lanes = new List<Road.Lane>();

            for (var index = 1; index <= numberOfLanes; index++)
            {
                var laneLeftPosition = lastRightPosition + moveDirection * laneWidth;
                var nodePosition = lastRightPosition + moveDirection * (laneWidth / 2);

                lanes.Add(new Road.Lane
                {
                    Heading = laneHeading,
                    RightSide = lastRightPosition,
                    LeftSide = laneLeftPosition,
                    NodePosition = nodePosition,
                    Width = laneWidth,
                    IsOppositeHeadingOfRoadNodeHeading = isOpposite
                });
                lastRightPosition = laneLeftPosition;
            }

            return lanes;
        }
    }
}