using System;
using System.Collections.Generic;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Street.Info;
using JetBrains.Annotations;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public interface IRoadblockSlot : IPreviewSupport, IDisposable
    {
        /// <summary>
        /// Get the position of the roadblock slot on the road.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// The offset position of this roadblock slot in regards to the road.
        /// </summary>
        Vector3 OffsetPosition { get; }

        /// <summary>
        /// Get the heading of the roadblock slot.
        /// </summary>
        float Heading { get; }

        /// <summary>
        /// Get the backup unit type that will be used for this slot.
        /// </summary>
        EBackupUnit BackupType { get; }

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
        /// The cop instances of this roadblock slot.
        /// </summary>
        IEnumerable<ARPed> Cops { get; }

        /// <summary>
        /// The game instances of this slot.
        /// </summary>
        List<InstanceSlot> Instances { get; }
        
        /// <summary>
        /// The length of the vehicle model in the slot.
        /// </summary>
        float VehicleLength { get; }

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

        /// <summary>
        /// Warp the cop peds in the slot vehicle.
        /// </summary>
        void WarpInVehicle();
    }
}