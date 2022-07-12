using System;
using System.Collections.Generic;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Dispatcher
{
    /// <summary>
    /// The roadblock dispatcher is responsible for determining & dispatching roadblocks.
    /// </summary>
    public interface IRoadblockDispatcher : IDisposable
    {
        /// <summary>
        /// Invoked when a roadblock cop is killed.
        /// </summary>
        event RoadblockEvents.RoadblockCopKilled RoadblockCopKilled;

        /// <summary>
        /// Invoked when a roadblock state has changed.
        /// </summary>
        event RoadblockEvents.RoadblockStateChanged RoadblockStateChanged;
        
        /// <summary>
        /// Get the roadblocks which have been dispatched.
        /// </summary>
        IEnumerable<IRoadblock> Roadblocks { get; }

        /// <summary>
        /// Dispatch a new roadblock for the current pursuit.
        /// Forcing the roadblock will disable all condition checks for a roadblock to spawn.
        /// </summary>
        /// <param name="level">The level of the roadblock. This determines the look/units/props of the roadblock.</param>
        /// <param name="vehicle">The vehicle for which a roadblock should be dispatched.</param>
        /// <param name="force">Set if a roadblock should be forced and no conditions should be checked.</param>
        /// <returns>Return true if a roadblock will be dispatched, else false.</returns>
        bool Dispatch(RoadblockLevel level, Vehicle vehicle, bool force = false);

        /// <summary>
        /// Dispatch a new roadblock preview for the given vehicle.
        /// </summary>
        /// <param name="level">The level of the roadblock. This determines the look/units/props of the roadblock.</param>
        /// <param name="vehicle">The vehicle for which a roadblock preview should be dispatched.</param>
        void DispatchPreview(RoadblockLevel level, Vehicle vehicle);
    }
}