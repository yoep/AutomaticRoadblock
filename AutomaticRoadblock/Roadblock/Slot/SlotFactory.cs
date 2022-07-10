using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public static class SlotFactory
    {
        /// <summary>
        /// Create a roadblock slot for the given level.
        /// </summary>
        /// <param name="level">The level to create a slot for.</param>
        /// <param name="position">The position of the slot.</param>
        /// <param name="heading">The heading of the slot.</param>
        /// <returns>Returns the created roadblock slot.</returns>
        public static IRoadblockSlot Create(RoadblockLevel level, Vector3 position, float heading)
        {
            if (level == RoadblockLevel.Level1)
            {
                return new RoadblockSlotLevel1(position, heading);
            }

            if (level == RoadblockLevel.Level2)
            {
                return new RoadblockSlotLevel2(position, heading);
            }

            return null;
        }
    }
}