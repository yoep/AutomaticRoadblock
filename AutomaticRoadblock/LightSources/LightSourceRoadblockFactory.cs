using System.Collections.Generic;
using AutomaticRoadblocks.Instance;
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
    }
}