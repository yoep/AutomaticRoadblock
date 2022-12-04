using System.Collections.Generic;
using AutomaticRoadblocks.Roadblock.Slot;
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
        /// Invoked when the slot state has changed.
        /// </summary>
        /// <param name="slot">The slot which state has been changed.</param>
        /// <param name="newState">The new state of the slot.</param>
        public delegate void RoadblockSlotStateChanged(IRoadblockSlot slot, ERoadblockSlotState newState);

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

        /// <summary>
        /// Invoked when the roadblock slot has been hit.
        /// </summary>
        /// <param name="slot">The roadblock slot that has been hit.</param>
        /// <param name="type">The target roadblock hit type.</param>
        public delegate void RoadblockSlotHit(IRoadblockSlot slot, ERoadblockHitType type);
    }
}