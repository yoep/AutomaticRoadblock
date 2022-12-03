using System;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using Rage;

namespace AutomaticRoadblocks.CloseRoad
{
    public interface ICloseRoadDispatcher : IDisposable
    {
        /// <summary>
        /// The backup unit which will be used for closing the road.
        /// </summary>
        EBackupUnit BackupUnit { get; set; }
        
        /// <summary>
        /// The light source to place during night.
        /// </summary>
        LightModel LightSource { get; set; }
        
        /// <summary>
        /// Close the nearby road around the given location.
        /// </summary>
        /// <param name="position">The position to close the road around.</param>
        /// <param name="preview">Close the nearby road as a preview only.</param>
        void CloseNearbyRoad(Vector3 position, bool preview = false);

        /// <summary>
        /// Opens all closed roads again.
        /// </summary>
        /// <param name="previewsOnly">Remove only the previews.</param>
        void OpenRoads(bool previewsOnly = false);
    }
}