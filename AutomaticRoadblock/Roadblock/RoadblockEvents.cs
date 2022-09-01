using System.Collections.Generic;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    public static class RoadblockEvents
    {
        /// <summary>
        /// Invoked when the roadblock state changes.
        /// </summary>
        /// <param name="roadblock">The roadblock of which the state changed.</param>
        /// <param name="newState">The new state of the roadblock.</param>
        public delegate void RoadblockStateChanged(IRoadblock roadblock, ERoadblockState newState);

        /// <summary>
        /// Invoked when the suspect has killed a cop from the roadblock.
        /// </summary>
        /// <param name="roadblock">The roadblock of which the cop was killed.</param>
        public delegate void RoadblockCopKilled(IRoadblock roadblock);

        /// <summary>
        /// Invoked when cops from a roadblock are joining the pursuit.
        /// </summary>
        /// <param name="roadblock">The roadblock of which the cops are joining the pursuit.</param>
        /// <param name="cops">The cops which join the pursuit.</param>
        public delegate void RoadblockCopsJoiningPursuit(IRoadblock roadblock, IEnumerable<Ped> cops);
    }
}