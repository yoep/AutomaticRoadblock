using System.Linq;

namespace AutomaticRoadblocks.Pursuit
{
    public class PursuitLevel
    {
        private const string PursuitLevelAudio = "ROADBLOCK_PURSUIT_LEVEL";
        
        public static readonly PursuitLevel Level1 = new(1, 0.1, 0.20, false);
        public static readonly PursuitLevel Level2 = new(2, 0.15, 0.15, false);
        public static readonly PursuitLevel Level3 = new(3, 0.2, 0.05, true);
        public static readonly PursuitLevel Level4 = new(4, 0.3, 0.025, true);
        public static readonly PursuitLevel Level5 = new(5, 0.35, 0.0, true);

        public static readonly PursuitLevel[] Levels =
        {
            Level1,
            Level2,
            Level3,
            Level4,
            Level5
        };

        private PursuitLevel(int level, double dispatchFactor, double automaticLevelIncreaseFactor, bool isLethalForceAllowed)
        {
            Level = level;
            DispatchFactor = dispatchFactor;
            AutomaticLevelIncreaseFactor = automaticLevelIncreaseFactor;
            IsLethalForceAllowed = isLethalForceAllowed;
        }

        /// <summary>
        /// Get the pursuit level.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Get the factor at which a roadblock can be dispatched for the pursuit level.
        /// </summary>
        public double DispatchFactor { get; }

        /// <summary>
        /// Get the factor at which the pursuit level can be increased automatically to the next level.
        /// </summary>
        public double AutomaticLevelIncreaseFactor { get; }

        /// <summary>
        /// Verify if firing weapons is allowed.
        /// </summary>
        public bool IsLethalForceAllowed { get; }

        /// <summary>
        /// Retrieve the audio file which should be played for this level.
        /// </summary>
        /// <remarks>Audio files for <see cref="Level1"/> are not available.</remarks>
        public string AudioFile => PursuitLevelAudio + Level;

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