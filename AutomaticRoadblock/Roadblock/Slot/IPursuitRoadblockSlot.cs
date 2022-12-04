using System.Collections.Generic;
using AutomaticRoadblocks.Instances;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public interface IPursuitRoadblockSlot : IRoadblockSlot
    {
        /// <summary>
        /// Verify if a cop from this slot has been killed by the target vehicle.
        /// </summary>
        bool HasCopBeenKilledByTarget { get; }
        
        /// <summary>
        /// The cop instances of the roadblock slot that will be joining the pursuit.
        /// </summary>
        IList<ARPed> CopsJoiningThePursuit { get; }

        /// <summary>
        /// Invoked when this slot has been hit by the target.
        /// </summary>
        event RoadblockEvents.RoadblockSlotHit RoadblockSlotHit;
    }
}