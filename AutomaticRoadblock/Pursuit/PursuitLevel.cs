using System.Linq;

namespace AutomaticRoadblocks.Pursuit
{
    public class PursuitLevel
    {
        public static readonly PursuitLevel Level1 = new PursuitLevel(1, false);
        public static readonly PursuitLevel Level2 = new PursuitLevel(2, false);
        public static readonly PursuitLevel Level3 = new PursuitLevel(3, true);
        public static readonly PursuitLevel Level4 = new PursuitLevel(4, true);
        public static readonly PursuitLevel Level5 = new PursuitLevel(5, true);

        public static readonly PursuitLevel[] Levels =
        {
            Level1,
            Level2,
            Level3,
            Level4,
            Level5
        };

        private PursuitLevel(int level, bool lethalAllowed)
        {
            Level = level;
            LethalAllowed = lethalAllowed;
        }

        /// <summary>
        /// Get the pursuit level.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Verify if firing weapons is allowed.
        /// </summary>
        public bool LethalAllowed { get; }

        public override string ToString()
        {
            return $"{nameof(Level)}: {Level}";
        }

        /// <summary>
        /// Get the pursuit level for the given level.
        /// </summary>
        /// <param name="level">The level between 1 and 5.</param>
        /// <returns>Returns the pursuit level.</returns>
        public static PursuitLevel From(int level)
        {
            return Levels
                .First(x => x.Level == level);
        }
    }
}