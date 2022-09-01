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
        /// Invoked when cops from a roadblock are joining the pursuit.
        /// </summary>
        event RoadblockEvents.RoadblockCopsJoiningPursuit RoadblockCopsJoiningPursuit;
        
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
        /// <param name="userRequested">Set if the roadblock is requested by the user.</param>
        /// <param name="force">Set if a roadblock should be forced and no conditions should be checked.</param>
        /// <param name="atCurrentLocation">Indicates if the roadblock should be dispatched at the vehicle's current location.</param>
        /// <returns>Return true if a roadblock will be dispatched, else false.</returns>
        bool Dispatch(RoadblockLevel level, Vehicle vehicle, bool userRequested, bool force = false, bool atCurrentLocation = false);

        /// <summary>
        /// Dispatch a new roadblock preview for the given vehicle.
        /// </summary>
        /// <param name="level">The level of the roadblock. This determines the look/units/props of the roadblock.</param>
        /// <param name="vehicle">The vehicle for which a roadblock preview should be dispatched.</param>
        /// <param name="atCurrentLocation">Set if the preview should be dispatched at the current vehicle location.</param>
        void DispatchPreview(RoadblockLevel level, Vehicle vehicle, bool atCurrentLocation = false);

        /// <summary>
        /// Dismiss any currently active roadblocks.
        /// This is most of the time used when a pursuit has ended or has changed to an on-foot chase.
        /// </summary>
        void DismissActiveRoadblocks();
    }
}