using System.Collections.Generic;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.LightSources
{
    /// <summary>
    /// A factory which produces light source instances for a <see cref="IRoadblock"/>.
    /// </summary>
    public static class LightSourceRoadblockFactory
    {
        public static IEnumerable<InstanceSlot> CreateGeneratorLights(IRoadblock roadblock)
        {
            Assert.NotNull(roadblock, "roadblock cannot be null");
            var roadRightSidePosition = roadblock.Road.RightSide + MathHelper.ConvertHeadingToDirection(roadblock.Heading) * 5f;
            var roadLeftSidePosition = roadblock.Road.LeftSide + MathHelper.ConvertHeadingToDirection(roadblock.Heading) * 5f;
            var targetPosition = roadblock.Position + MathHelper.ConvertHeadingToDirection(roadblock.Heading - 180) * 5f;

            return new List<InstanceSlot>
            {
                new(EntityType.Scenery, roadRightSidePosition, MathHelper.ConvertDirectionToHeading(targetPosition - roadRightSidePosition),
                    (position, heading) => new ARScenery(PropUtils.CreateGeneratorWithLights(position, heading))),
                new(EntityType.Scenery, roadLeftSidePosition, MathHelper.ConvertDirectionToHeading(targetPosition - roadLeftSidePosition),
                    (position, heading) => new ARScenery(PropUtils.CreateGeneratorWithLights(position, heading)))
            };
        }

        public static IEnumerable<InstanceSlot> CreateAlternatingGroundLights(IRoadblock roadblock, int numberOfLaneLights)
        {
            Assert.NotNull(roadblock, "roadblock cannot be null");
            var rightSide = roadblock.Road.RightSide + MathHelper.ConvertHeadingToDirection(roadblock.Heading - 90) * 2f;
            var leftSide = roadblock.Road.LeftSide + MathHelper.ConvertHeadingToDirection(roadblock.Heading + 90) * 2f;
            var placementDirection = MathHelper.ConvertHeadingToDirection(roadblock.Heading - 180);
            var instances = new List<InstanceSlot>();
            var initialDistance = 3f;

            for (var i = 0; i < numberOfLaneLights; i++)
            {
                var roadRightSidePosition = rightSide + placementDirection * initialDistance;
                var roadLeftSidePosition = leftSide + placementDirection * initialDistance;
                var unmodifiedCount = i;

                instances.AddRange(new List<InstanceSlot>
                {
                    new(EntityType.Scenery, roadRightSidePosition, roadblock.Heading,
                        (position, _) =>
                            new ARScenery(unmodifiedCount % 2 == 0 ? PropUtils.CreateBlueStandingGroundLight(position) : PropUtils.CreateRedStandingGroundLight(position))),
                    new(EntityType.Scenery, roadLeftSidePosition, roadblock.Heading,
                        (position, _) =>
                            new ARScenery(unmodifiedCount % 2 == 0 ? PropUtils.CreateBlueStandingGroundLight(position) : PropUtils.CreateRedStandingGroundLight(position)))
                });

                initialDistance += 2.5f;
            }

            return instances;
        }

        public static IEnumerable<InstanceSlot> CreateRedBlueGroundLights(IRoadblock roadblock, int numberOfLaneLights)
        {
            Assert.NotNull(roadblock, "roadblock cannot be null");
            var rightSide = roadblock.Road.RightSide + MathHelper.ConvertHeadingToDirection(roadblock.Heading - 90) * 2f;
            var leftSide = roadblock.Road.LeftSide + MathHelper.ConvertHeadingToDirection(roadblock.Heading + 90) * 2f;
            var placementDirection = MathHelper.ConvertHeadingToDirection(roadblock.Heading - 180);
            var instances = new List<InstanceSlot>();
            var initialDistance = 3f;

            for (var i = 0; i < numberOfLaneLights; i++)
            {
                var roadRightSidePosition = rightSide + placementDirection * initialDistance;
                var roadLeftSidePosition = leftSide + placementDirection * initialDistance;

                instances.AddRange(new List<InstanceSlot>
                {
                    new(EntityType.Scenery, roadRightSidePosition, roadblock.Heading,
                        (position, heading) => new ARScenery(PropUtils.CreateConeWithLight(position, heading + 25))),
                    new(EntityType.Scenery, roadLeftSidePosition, roadblock.Heading,
                        (position, heading) => new ARScenery(PropUtils.CreateConeWithLight(position, heading - 25)))
                });

                initialDistance += 2.5f;
            }

            return instances;
        }
    }
}