using Rage;

namespace AutomaticRoadblocks.Roadblock.Dispatcher
{
    internal class RoadblockInfo
    {
        private const float DistanceTolerance = 10f;
        
        public RoadblockInfo(IRoadblock roadblock)
        {
            Roadblock = roadblock;
        }

        /// <summary>
        /// The roadblock to which the info applies.
        /// </summary>
        public IRoadblock Roadblock { get; }

        /// <summary>
        /// The position of the <see cref="Roadblock"/>.
        /// </summary>
        public Vector3 Position => Roadblock.Position;

        /// <summary>
        /// The state of the <see cref="Roadblock"/>
        /// </summary>
        public RoadblockState State => Roadblock.State;

        /// <summary>
        /// The last known distance of the <see cref="Roadblock"/> in regards to the player.
        /// </summary>
        private float LastKnownDistanceFromPlayer { get; set; } = 9999f;

        /// <summary>
        /// Verify if the player is moving towards the <see cref="Roadblock"/>.
        /// This method stores the given distance in for the next check.
        /// </summary>
        /// <param name="newDistanceToRoadblock">The new distance of the player towards the roadblock.</param>
        /// <returns>Returns true if the player is moving towards the roadblock, else false.</returns>
        /// <remarks>This should prevent roadblocks from being cleaned when it was spawned far away from the player due to the suspect
        /// getting far ahead of the player.</remarks>
        public bool IsPlayerMovingTowardsRoadblock(float newDistanceToRoadblock)
        {
            var isPlayerMovingTowardsRoadblock = newDistanceToRoadblock - LastKnownDistanceFromPlayer <= DistanceTolerance;
            LastKnownDistanceFromPlayer = newDistanceToRoadblock;
            
            return isPlayerMovingTowardsRoadblock;
        }
    }
}