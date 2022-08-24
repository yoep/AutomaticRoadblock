using System;
using System.Collections.Generic;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.LightSources
{
    /// <summary>
    /// A factory which creates light sources for a <see cref="IRoadblockSlot"/>
    /// </summary>
    public static class LightSourceSlotFactory
    {
        private static readonly Random Random = new();

        public static IEnumerable<InstanceSlot> CreateFlares(IRoadblockSlot roadblockSlot)
        {
            Assert.NotNull(roadblockSlot, "roadblockSlot cannot be null");
            var logger = IoC.Instance.GetInstance<ILogger>();
            var rowPosition = roadblockSlot.Position + MathHelper.ConvertHeadingToDirection(roadblockSlot.Heading - 180) * 2f;
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(roadblockSlot.Heading + 90) * 3f;
            var direction = MathHelper.ConvertHeadingToDirection(roadblockSlot.Heading - 90);
            var totalFlares = (int)roadblockSlot.Lane.Width;
            var instances = new List<InstanceSlot>();

            logger.Debug($"Creating a total of {totalFlares} flares for the roadblock slot");
            for (var i = 0; i < totalFlares; i++)
            {
                instances.Add(new InstanceSlot(EntityType.Scenery, startPosition, roadblockSlot.Heading,
                    (position, heading) => new ARScenery(PropUtils.CreateHorizontalFlare(position, heading + Random.Next(360)))));
                startPosition += direction * 1f;
            }

            return instances;
        }
    }
}