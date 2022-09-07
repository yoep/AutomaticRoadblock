namespace AutomaticRoadblocks.Roadblock
{
    public class ERoadblockLevel
    {
        public static readonly ERoadblockLevel None = new(-1);
        public static readonly ERoadblockLevel Level1 = new(1);
        public static readonly ERoadblockLevel Level2 = new(2);
        public static readonly ERoadblockLevel Level3 = new(3);
        public static readonly ERoadblockLevel Level4 = new(4);
        public static readonly ERoadblockLevel Level5 = new(5);

        public static readonly ERoadblockLevel[] Levels =
        {
            Level1,
            Level2,
            Level3,
            Level4,
            Level5
        };

        private ERoadblockLevel(int level)
        {
            Level = level;
        }

        /// <summary>
        /// Verify if this roadblock level is the <see cref="None"/> type.
        /// </summary>
        public bool IsNone => this == None;

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