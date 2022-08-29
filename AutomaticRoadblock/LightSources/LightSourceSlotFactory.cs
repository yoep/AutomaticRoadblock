using System;
using System.Collections.Generic;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils;
using Rage;
using Object = Rage.Object;

namespace AutomaticRoadblocks.LightSources
{
    /// <summary>
    /// A factory which creates light sources for a <see cref="IRoadblockSlot"/>
    /// </summary>
    public static class LightSourceSlotFactory
    {
        private static readonly Random Random = new();

        private static readonly Dictionary<LightSourceType, Func<IRoadblockSlot, IEnumerable<InstanceSlot>>> LightSources = new()
        {
            { LightSourceType.None, _ => new List<InstanceSlot>() },
            { LightSourceType.Spots, _ => new List<InstanceSlot>() },
            { LightSourceType.Flares, slot => DoInternalCreation(slot, LightSourceType.Flares.Spacing, PropUtils.CreateHorizontalFlare) },
            { LightSourceType.Warning, slot => DoInternalCreation(slot, LightSourceType.Warning.Spacing, PropUtils.CreateWarningLight) },
            { LightSourceType.BlueStanding, slot => DoInternalCreation(slot, LightSourceType.BlueStanding.Spacing, PropUtils.CreateBlueStandingGroundLight) },
            { LightSourceType.RedStanding, slot => DoInternalCreation(slot, LightSourceType.RedStanding.Spacing, PropUtils.CreateRedStandingGroundLight) },
        };

        public static IEnumerable<InstanceSlot> Create(LightSourceType lightSourceType, IRoadblockSlot roadblockSlot)
        {
            Assert.NotNull(lightSourceType, "lightSourceType cannot be null");
            Assert.NotNull(roadblockSlot, "roadblockSlot cannot be null");
            return LightSources[lightSourceType].Invoke(roadblockSlot);
        }

        private static IEnumerable<InstanceSlot> DoInternalCreation(IRoadblockSlot roadblockSlot, float spacing, Func<Vector3, float, Object> propFactory)
        {
            var rowPosition = CalculateRowPosition(roadblockSlot);
            var startPosition = CalculateStartPositionOfFirstLight(roadblockSlot, rowPosition);
            var direction = CalculateDirectionOfLightPlacements(roadblockSlot);
            var totalWarning = (int)(roadblockSlot.Lane.Width / spacing);
            var instances = new List<InstanceSlot>();

            for (var i = 0; i < totalWarning; i++)
            {
                instances.Add(new InstanceSlot(EntityType.Scenery, startPosition, roadblockSlot.Heading,
                    (position, heading) => new ARScenery(propFactory.Invoke(position, heading + Random.Next(360)))));
                startPosition += direction * spacing;
            }

            return instances;
        }

        private static Vector3 CalculateDirectionOfLightPlacements(IRoadblockSlot roadblockSlot)
        {
            return MathHelper.ConvertHeadingToDirection(roadblockSlot.Heading - 90);
        }

        private static Vector3 CalculateStartPositionOfFirstLight(IRoadblockSlot roadblockSlot, Vector3 rowPosition)
        {
            return rowPosition + MathHelper.ConvertHeadingToDirection(roadblockSlot.Heading + 90) * (roadblockSlot.Lane.Width / 2);
        }

        private static Vector3 CalculateRowPosition(IRoadblockSlot roadblockSlot)
        {
            return roadblockSlot.Position + MathHelper.ConvertHeadingToDirection(roadblockSlot.Heading - 180) * 2f;
        }
    }
}