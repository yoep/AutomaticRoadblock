using System;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
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
        RoadblockState State { get; }

        /// <summary>
        /// Invoked when the roadblock state changes.
        /// </summary>
        event RoadblockEvents.RoadblockStateChanged RoadblockStateChanged;

        /// <summary>
        /// Invoked when a cop from the roadblock is killed.
        /// </summary>
        event RoadblockEvents.RoadblockCopKilled RoadblockCopKilled;
        
        /// <summary>
        /// Invoked when cops from a roadblock are joining the pursuit.
        /// </summary>
        event RoadblockEvents.RoadblockCopsJoiningPursuit RoadblockCopsJoiningPursuit;

        /// <summary>
        /// Spawn the roadblock in the world.
        /// </summary>
        void Spawn();

        /// <summary>
        /// Release the roadblock instance back to the world.
        /// This can only be used when the <see cref="State"/> is <see cref="RoadblockState.Active"/>.
        /// </summary>
        void Release();
    }
}