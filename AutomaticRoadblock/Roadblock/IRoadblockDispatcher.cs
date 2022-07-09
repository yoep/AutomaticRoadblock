using System;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    /// <summary>
    /// The roadblock dispatcher is responsible for determining & dispatching roadblocks.
    /// </summary>
    public interface IRoadblockDispatcher : IDisposable
    {
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