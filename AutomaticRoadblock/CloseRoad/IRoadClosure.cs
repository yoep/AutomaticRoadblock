using System;
using AutomaticRoadblocks.Preview;

namespace AutomaticRoadblocks.CloseRoad
{
    public interface IRoadClosure : IDisposable, IPreviewSupport
    {
        /// <summary>
        /// Spawn the close road instances.
        /// </summary>
        void Spawn();

        /// <summary>
        /// Release the close road instances back to LSPDFR.
        /// </summary>
        void Release();
    }
}