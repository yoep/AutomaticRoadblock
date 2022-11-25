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
        // 0001
        DetectBypass = 1,

        /// <summary>
        /// Detect if the target vehicle has hit the roadblock.
        /// </summary>
        // 0010
        DetectHit = 2,
        
        DetectAll = DetectBypass | DetectHit,

        /// <summary>
        /// Indicates if the roadblock is allowed to join the pursuit on hit.
        /// Auto enables the <see cref="DetectHit"/>.
        /// </summary>
        // 0110
        JoinPursuitOnHit = 6,
        
        /// <summary>
        /// Indicates if the roadblock is allowed to join the pursuit on bypass.
        /// Auto enables the <see cref="DetectBypass"/>.
        /// </summary>
        // 1001
        JoinPursuitOnBypass = 9,
        
        // 1111
        JoinPursuit = JoinPursuitOnHit | JoinPursuitOnBypass,

        /// <summary>
        /// Indicates if the audio should be played for the roadblock.
        /// </summary>
        PlayAudio = 16,
        
        /// <summary>
        /// Indicates if the speed around the roadblock should be limited.
        /// This will slow down the traffic in the area.
        /// </summary>
        SlowTraffic = 32,
        
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