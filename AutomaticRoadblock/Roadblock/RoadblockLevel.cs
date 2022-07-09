namespace AutomaticRoadblocks.Roadblock
{
    public class RoadblockLevel
    {
        public static readonly RoadblockLevel Level1 = new RoadblockLevel(1);
        public static readonly RoadblockLevel Level2 = new RoadblockLevel(2);
        public static readonly RoadblockLevel Level3 = new RoadblockLevel(3);
        public static readonly RoadblockLevel Level4 = new RoadblockLevel(4);
        public static readonly RoadblockLevel Level5 = new RoadblockLevel(5);

        public static readonly RoadblockLevel[] Levels =
        {
            Level1,
            Level2,
            Level3,
            Level4,
            Level5,
        };

        private RoadblockLevel(int level)
        {
            Level = level;
        }

        /// <summary>
        /// Get the level of the roadblock.
        /// </summary>
        public int Level { get; }

        public override string ToString()
        {
            return $"{Level}";
        }
    }
}