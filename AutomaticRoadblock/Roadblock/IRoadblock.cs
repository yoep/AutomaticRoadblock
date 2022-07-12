using System;
using AutomaticRoadblocks.Preview;

namespace AutomaticRoadblocks.Roadblock
{
    public interface IRoadblock : IDisposable, IPreviewSupport
    {
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
        /// Spawn the roadblock in the world.
        /// </summary>
        void Spawn();
    }
}