using System;
using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using JetBrains.Annotations;
using Rage;

namespace AutomaticRoadblocks.CloseRoad
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
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
        /// If the position is at an intersection, all roads towards the intersection will be closed.
        /// If the position is a road, each possible driving direction will be closed away from the original position and stopped at the first intersection
        /// if any are encountered along the road.
        /// </summary>
        /// <param name="position">The position to close the road around.</param>
        /// <param name="preview">Close the nearby road as a preview only.</param>
        /// <returns>Returns the road closure on success, else null when unable to close road at given location.</returns>
        [CanBeNull]
        IRoadClosure CloseNearbyRoad(Vector3 position, bool preview = false);

        /// <summary>
        /// Close the nearby road around the given location.
        /// If the position is at an intersection, all roads towards the intersection will be closed.
        /// If the position is a road, each possible driving direction will be closed away from the original position and stopped at the first intersection
        /// if any are encountered along the road.
        /// </summary>
        /// <param name="position">The position to close the road around.</param>
        /// <param name="backupUnit">The backup unit to use within the road closure.</param>
        /// <param name="barrier">The barrier to place along the road closure.</param>
        /// <param name="lightSource">The light source to place along the closed road instances.</param>
        /// <param name="maxDistance">The maximum distance from the position the units can be placed.</param>
        /// <param name="preview">Close the nearby road as a preview only.</param>
        /// <returns>Returns the road closure on success, else null when unable to close road at given location.</returns>
        IRoadClosure CloseNearbyRoad(Vector3 position, EBackupUnit backupUnit, BarrierModel barrier, LightModel lightSource, float maxDistance, bool preview = false);

        /// <summary>
        /// Opens all closed roads again.
        /// </summary>
        /// <param name="previewsOnly">Remove only the previews.</param>
        void OpenRoads(bool previewsOnly = false);
    }
}