using System;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Utils.Road;
using JetBrains.Annotations;
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
        /// Retrieve the vehicle model that is used within this slot.
        /// </summary>
        Model VehicleModel { get; }

        /// <summary>
        /// Retrieve the vehicle instance (nullable) of this slot.
        /// It returns null when <see cref="Spawn"/> has not been called yet.
        /// </summary>
        [CanBeNull]
        Vehicle Vehicle { get; }

        /// <summary>
        /// The lane this slot blocks of a certain road.
        /// </summary>
        [NotNull]
        Road.Lane Lane { get; }

        /// <summary>
        /// Spawn the slot entities into the world.
        /// </summary>
        void Spawn();

        /// <summary>
        /// Modify/move the current position of the vehicle within this slot.
        /// </summary>
        /// <param name="newPosition">The new position of the vehicle within the slot.</param>
        /// <remarks>It's recommended to NOT use this method when this slot has been <see cref="Spawn"/>.</remarks>
        void ModifyVehiclePosition(Vector3 newPosition);

        /// <summary>
        /// Release the cop entities to LSPDFR.
        /// This allows the cops to join the pursuit.
        /// </summary>
        void Release();
    }
}