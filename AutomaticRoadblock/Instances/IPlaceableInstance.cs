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

        /// <summary>
        /// Release the instance back to the world.
        /// </summary>
        /// <param name="releaseAll">Indicates of all cop instances should be released.</param>
        void Release(bool releaseAll = false);
    }
}