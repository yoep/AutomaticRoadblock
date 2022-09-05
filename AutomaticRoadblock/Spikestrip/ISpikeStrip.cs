using System;
using AutomaticRoadblocks.Preview;
using Rage;

namespace AutomaticRoadblocks.SpikeStrip
{
    public interface ISpikeStrip : IPreviewSupport, IDisposable
    {
        /// <summary>
        /// The position of the spike strip.
        /// </summary>
        Vector3 Position { get; }
        
        /// <summary>
        /// The heading of the spike strip.
        /// </summary>
        float Heading { get; }
        
        /// <summary>
        /// The location of the spike strip on the road.
        /// </summary>
        ESpikeStripLocation Location { get; }
        
        /// <summary>
        /// The state of the spike strip.
        /// </summary>
        ESpikeStripState State { get; }
        
        /// <summary>
        /// Invoked when the state is changed.
        /// </summary>
        event SpikeStripEvents.SpikeStripStateChanged StateChanged;

        /// <summary>
        /// Spawn the spike strip.
        /// This will spawn the entity into the game world as <see cref="ESpikeStripState.Undeployed"/>.
        /// </summary>
        void Spawn();
        
        /// <summary>
        /// Deploy the spike strip.
        /// This action is ignored if the state is <see cref="ESpikeStripState.Deploying"/> or <see cref="ESpikeStripState.Deployed"/>.
        /// </summary>
        void Deploy();
    }
}