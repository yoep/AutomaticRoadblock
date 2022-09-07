using System;

namespace AutomaticRoadblocks.Roadblock
{
    [Flags]
    public enum ERoadblockFlags
    {
        None = 0,

        /// <summary>
        /// Detect if the target vehicle has bypassed the roadblock.
        /// </summary>
        DetectBypass = 1,

        /// <summary>
        /// Detect if the target vehicle has hit the roadblock.
        /// </summary>
        DetectHit = 2,
        
        DetectAll = DetectBypass | DetectHit,

        /// <summary>
        /// Indicates if the roadblock is allowed to join the pursuit on hit.
        /// Auto enables the <see cref="DetectHit"/>.
        /// </summary>
        JoinPursuitOnHit = 6,
        
        /// <summary>
        /// Indicates if the roadblock is allowed to join the pursuit on bypass.
        /// Auto enables the <see cref="DetectBypass"/>.
        /// </summary>
        JoinPursuitOnBypass = 9,
        
        JoinPursuit = JoinPursuitOnHit | JoinPursuitOnBypass,

        /// <summary>
        /// Indicates if the audio should be played for the roadblock.
        /// </summary>
        PlayAudio = 16,
        
        /// <summary>
        /// Indicates if the speed around the roadblock should be limited.
        /// </summary>
        LimitSpeed = 32,
        
        /// <summary>
        /// Enable lights within the roadblock.
        /// </summary>
        EnableLights = 64,
        
        /// <summary>
        /// Enable the placement of spike strips in the roadblock.
        /// </summary>
        EnableSpikeStrips = 128,
        
        /// <summary>
        /// Force the cop peds to be warped inside the vehicle.
        /// </summary>
        ForceInVehicle = 256,
    }
}