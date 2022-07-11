using System;
using System.Collections.Generic;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public static class SlotFactory
    {
        private static readonly IDictionary<RoadblockLevel, Func<Vector3, float, IRoadblockSlot>> Levels =
            new Dictionary<RoadblockLevel, Func<Vector3, float, IRoadblockSlot>>
            {
                { RoadblockLevel.Level1, (position, heading) => new RoadblockSlotLevel1(position, heading) },
                { RoadblockLevel.Level2, (position, heading) => new RoadblockSlotLevel2(position, heading) },
                { RoadblockLevel.Level3, (position, heading) => new RoadblockSlotLevel3(position, heading) },
                { RoadblockLevel.Level4, (position, heading) => new RoadblockSlotLevel4(position, heading) },
                { RoadblockLevel.Level5, (position, heading) => new RoadblockSlotLevel5(position, heading) }
            };

        /// <summary>
        /// Create a roadblock slot for the given level.
        /// </summary>
        /// <param name="level">The level to create a slot for.</param>
        /// <param name="position">The position of the slot.</param>
        /// <param name="heading">The heading of the slot.</param>
        /// <returns>Returns the created roadblock slot.</returns>
        public static IRoadblockSlot Create(RoadblockLevel level, Vector3 position, float heading)
        {
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(position, "position cannot be null");
            return Levels[level].Invoke(position, heading);
        }
    }
}