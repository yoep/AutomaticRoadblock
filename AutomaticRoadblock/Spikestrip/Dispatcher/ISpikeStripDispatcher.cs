using System;
using Rage;

namespace AutomaticRoadblocks.SpikeStrip.Dispatcher
{
    public interface ISpikeStripDispatcher : IDisposable
    {
        /// <summary>
        /// Deploy a spike strip.
        /// </summary>
        /// <param name="position">The position to deploy a spike strip at.</param>
        void Deploy(Vector3 position);

        /// <summary>
        /// Remove all spike strips.
        /// </summary>
        void RemoveAll();
    }
}