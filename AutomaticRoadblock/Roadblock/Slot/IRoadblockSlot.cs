using System;
using AutomaticRoadblocks.Preview;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public interface IRoadblockSlot : IPreviewSupport, IDisposable
    {
        /// <summary>
        /// Get the position of the roadblock slot.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Get the heading of the roadblock slot.
        /// </summary>
        float Heading { get; }

        /// <summary>
        /// Get the vehicle instance of this slot.
        /// It returns null when <see cref="Spawn"/> has not been called yet.
        /// </summary>
        Vehicle Vehicle { get; }

        /// <summary>
        /// Invoked when a cop from this slot has been killed.
        /// </summary>
        event RoadblockEvents.RoadblockSlotEvents.RoadblockCopKilled RoadblockCopKilled;

        /// <summary>
        /// Spawn the slot entities into the world.
        /// </summary>
        void Spawn();

        /// <summary>
        /// Release the AI entities to LSPDFR.
        /// This allows the cops to join the pursuit.
        /// </summary>
        void ReleaseToLspdfr();
    }
}