using System.Collections.Generic;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Street.Factory
{
    internal static class RoadFactory
    {
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        internal static Road Create(VehicleNodeInfo nodeInfo)
        {
            var nodeHeading = nodeInfo.Heading;
            var rightSideHeading = nodeHeading - 90f;
            var leftSideHeading = nodeHeading + 90f;

            if (!StreetHelper.LastPointOnRoadUsingRaytracing(nodeInfo.Position, rightSideHeading, out var roadRightSide))
            {
                StreetHelper.LastPointOnRoadUsingNative(nodeInfo.Position, rightSideHeading, out var roadRightSideNative);
                roadRightSide = roadRightSideNative;
                Logger.Info(
                    $"Using native function right side last point {roadRightSide} instead (distance from center {nodeInfo.Position.DistanceTo(roadRightSide)})");
            }

            if (!StreetHelper.LastPointOnRoadUsingRaytracing(nodeInfo.Position, leftSideHeading, out var roadLeftSide))
            {
                StreetHelper.LastPointOnRoadUsingNative(nodeInfo.Position, leftSideHeading, out var roadLeftSideNative);
                roadLeftSide = roadLeftSideNative;
                Logger.Info(
                    $"Using native function left side last point {roadLeftSide} instead (distance from center {nodeInfo.Position.DistanceTo(roadLeftSide)})");
            }

            return new Road
            {
                RightSide = roadRightSide,
                LeftSide = roadLeftSide,
                Lanes = DiscoverLanes(roadRightSide, roadLeftSide, nodeInfo.Position, nodeHeading, nodeInfo.LanesInSameDirection,
                    nodeInfo.LanesInOppositeDirection),
                Node = nodeInfo,
            };
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