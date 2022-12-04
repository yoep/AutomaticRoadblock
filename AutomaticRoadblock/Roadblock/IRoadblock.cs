using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    /// <summary>
    /// A basic roadblock which has the ability to close a road with one or more slots.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IRoadblock : IDisposable, IPreviewSupport
    {
        /// <summary>
        /// Retrieve the position of the roadblock. 
        /// </summary>
        Vector3 Position { get; }
        
        /// <summary>
        /// Retrieve the heading of the roadblock.
        /// </summary>
        float Heading { get; }

        /// <summary>
        /// Retrieve the road that this roadblock is blocking.
        /// </summary>
        Road Road { get; }

        /// <summary>
        /// Get the last game time the state of this roadblock was changed.
        /// </summary>
        uint LastStateChange { get; }

        /// <summary>
        /// Get the state of the roadblock.
        /// </summary>
        ERoadblockState State { get; }
        
        /// <summary>
        /// The roadblock flags.
        /// </summary>
        ERoadblockFlags Flags { get; }
        
        /// <summary>
        /// The number of slots within the roadblock.
        /// </summary>
        int NumberOfSlots { get; }
        
        /// <summary>
        /// The cop instances within the roadblock.
        /// Cops released to LSPDFR won't be included anymore.
        /// </summary>
        IEnumerable<ARPed> Cops { get; }
        
        /// <summary>
        /// The vehicle instances within the roadblock.
        /// Vehicles released to LSPDFR won't be included anymore.
        /// </summary>
        IEnumerable<ARVehicle> Vehicles { get; }
        
        /// <summary>
        /// The slots within this roadblock.
        /// Each slot blocks a lane within the road and has a specific functionality.
        /// </summary>
        IEnumerable<IRoadblockSlot> Slots { get; }

        /// <summary>
        /// Invoked when the roadblock state changes.
        /// </summary>
        event RoadblockEvents.RoadblockStateChanged RoadblockStateChanged;

        /// <summary>
        /// Spawn the roadblock in the world.
        /// </summary>
        /// <returns>Returns true when all instances could be spawned with success, else false.</returns>
        bool Spawn();

        /// <summary>
        /// Release the roadblock instance back to the world.
        /// This can only be used when the <see cref="State"/> is <see cref="ERoadblockState.Active"/>.
        /// </summary>
        /// <param name="releaseAll">Indicates of all cop instances should be released.</param>
        void Release(bool releaseAll = false);
    }
}