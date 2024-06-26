using AutomaticRoadblocks.Roadblock;
using Rage;

namespace AutomaticRoadblocks.Pursuit
{
    /// <summary>
    /// Manages the roadblocks which are deployed during a pursuit.
    /// </summary>
    public interface IPursuitManager : IOnDutyListener
    {
        /// <summary>
        /// Verify if there is currently an active pursuit which is tracked by this manager.
        /// </summary>
        bool IsPursuitActive { get; }

        /// <summary>
        /// Enable automatic dispatching of roadblocks during a pursuit.
        /// </summary>
        bool EnableAutomaticDispatching { get; set; }

        /// <summary>
        /// Enable automatic level increases during a pursuit.
        /// </summary>
        bool EnableAutomaticLevelIncreases { get; set; }

        /// <summary>
        /// Enable spike strip to be deployed along the roadblock.
        /// </summary>
        bool EnableSpikeStrips { get; set; }

        /// <summary>
        /// The current state of the pursuit.
        /// </summary>
        EPursuitState State { get; }

        /// <summary>
        /// Retrieve the current pursuit level.
        /// </summary>
        PursuitLevel PursuitLevel { get; set; }

        /// <summary>
        /// Invoked when the pursuit state changes.
        /// </summary>
        event PursuitEvents.PursuitStateChangedEventHandler PursuitStateChanged;

        /// <summary>
        /// Invoked when the pursuit level changes.
        /// </summary>
        event PursuitEvents.PursuitLevelChangedEventHandler PursuitLevelChanged;

        /// <summary>
        /// Retrieve a vehicle of one of the suspects within the pursuit.
        /// Verify if there is an active pursuit through <see cref="IsPursuitActive"/>,
        /// otherwise, this method will return null.
        /// </summary>
        /// <returns>Returns a vehicle in the pursuit, else null when there is no active pursuit.</returns>
        Vehicle GetSuspectVehicle();

        /// <summary>
        /// Dispatch a roadblock for the current pursuit.
        /// </summary>
        /// <param name="userRequested">Indicates if the roadblock is requested by the user.</param>
        /// <param name="force">Force the spawning of a roadblock, this will disable the verification of conditions which are applied before a roadblock can be dispatched.</param>
        /// <param name="roadblockDistance">The placement distance if the roadblock.</param>
        /// <returns>Returns true if a roadblock will be dispatched, else false.</returns>
        /// <remarks>Execute this method on a separate fiber.</remarks>
        bool DispatchNow(bool userRequested = false, bool force = false, ERoadblockDistance roadblockDistance = ERoadblockDistance.Default);

        /// <summary>
        /// Dispatch a preview of a possible roadblock.
        /// This preview doesn't require an active pursuit, but required that the player has at least a vehicle to determine the roadblock location.
        /// </summary>
        /// <remarks>Execute this method on a separate fiber.</remarks>
        void DispatchPreview(ERoadblockDistance roadblockDistance);
    }
}