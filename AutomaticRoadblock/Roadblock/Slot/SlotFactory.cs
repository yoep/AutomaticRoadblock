using System;
using System.Collections.Generic;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public static class SlotFactory
    {
        private static readonly IDictionary<RoadblockLevel, Func<Vector3, float, Vehicle, IRoadblockSlot>> Levels =
            new Dictionary<RoadblockLevel, Func<Vector3, float, Vehicle, IRoadblockSlot>>
            {
                { RoadblockLevel.Level1, (position, heading, targetVehicle) => new RoadblockSlotLevel1(position, heading, targetVehicle) },
                { RoadblockLevel.Level2, (position, heading, targetVehicle) => new RoadblockSlotLevel2(position, heading, targetVehicle) },
                { RoadblockLevel.Level3, (position, heading, targetVehicle) => new RoadblockSlotLevel3(position, heading, targetVehicle) },
                { RoadblockLevel.Level4, (position, heading, targetVehicle) => new RoadblockSlotLevel4(position, heading, targetVehicle) },
                { RoadblockLevel.Level5, (position, heading, targetVehicle) => new RoadblockSlotLevel5(position, heading, targetVehicle) }
            };

        /// <summary>
        /// Create a roadblock slot for the given level.
        /// </summary>
        /// <param name="level">The level to create a slot for.</param>
        /// <param name="position">The position of the slot.</param>
        /// <param name="heading">The heading of the slot.</param>
        /// <returns>Returns the created roadblock slot.</returns>
        public static IRoadblockSlot Create(RoadblockLevel level, Vector3 position, float heading, Vehicle targetVehicle)
        {
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(position, "position cannot be null");
            return Levels[level].Invoke(position, heading, targetVehicle);
        }
    }
}