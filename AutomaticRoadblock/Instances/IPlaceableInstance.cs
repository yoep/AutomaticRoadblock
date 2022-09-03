using System;
using AutomaticRoadblocks.Preview;
using Rage;

namespace AutomaticRoadblocks.Instances
{
    public interface IPlaceableInstance : IPreviewSupport, IDisposable
    {
        /// <summary>
        /// The position of the instance.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Spawn the instance.
        /// </summary>
        /// <returns>Returns true when all instances spawned with success, else false.</returns>
        bool Spawn();
    }
}