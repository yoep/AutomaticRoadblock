using System;
using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.ManualPlacement
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public interface IManualPlacement : IPreviewSupport, IDisposable
    {
        /// <summary>
        /// The first row barrier to place.
        /// </summary>
        BarrierModel MainBarrier { get; set; }

        /// <summary>
        /// The second row barrier to place.
        /// </summary>
        BarrierModel SecondaryBarrier { get; set; }

        /// <summary>
        /// The backup type to use within the roadblock.
        /// </summary>
        EBackupUnit BackupType { get; set; }

        /// <summary>
        /// The light source type to use within the roadblock.
        /// </summary>
        LightModel LightSourceType { get; set; }

        /// <summary>
        /// The placement type of the roadblock.
        /// </summary>
        PlacementType PlacementType { get; set; }

        /// <summary>
        /// The placement direction of the roadblock.
        /// </summary>
        PlacementDirection Direction { get; set; }

        /// <summary>
        /// The indication if cops should be added to the roadblock.
        /// </summary>
        bool CopsEnabled { get; set; }

        /// <summary>
        /// The offset of the placement in regards to the node.
        /// </summary>
        float Offset { get; set; }

        /// <summary>
        /// Place a roadblock based on the <see cref="DetermineLocation"/> <see cref="Road"/>.
        /// </summary>
        [Obsolete("Use PlaceRoadblock(Vector3) instead")]
        void PlaceRoadblock();

        /// <summary>
        /// Place a roadblock at the given position which is not part of a pursuit.
        /// The nearest vehicle node to the position will be used to create a roadblock.
        /// </summary>
        /// <param name="position">The position to use for placing a roadblock.</param>
        /// <returns>Returns the created roadblock.</returns>
        IRoadblock PlaceRoadblock(Vector3 position);

        /// <summary>
        /// Place a roadblock at the given position which is not part of a pursuit.
        /// The nearest vehicle node to the position will be used to create a roadblock.
        /// </summary>
        /// <param name="position">The position of the roadblock to create.</param>
        /// <param name="targetHeading">The heading towards which the roadblock should be placed.</param>
        /// <param name="backupType">The backup unit type to use within the roadblock.</param>
        /// <param name="mainBarrier">The barriers in front of the roadblock to use.</param>
        /// <param name="secondaryBarrier">The barriers to use behind the roadblock.</param>
        /// <param name="lightSource">The light source to use within the roadblock.</param>
        /// <param name="placementType">The placement type of the roadblock.</param>
        /// <param name="copsEnabled">Indicates if cops should be placed within the roadblock.</param>
        /// <param name="offset">The position offset in regards to the closest vehicle node.</param>
        /// <returns>Returns the created roadblock.</returns>
        IRoadblock PlaceRoadblock(Vector3 position, float targetHeading, EBackupUnit backupType, BarrierModel mainBarrier, BarrierModel secondaryBarrier, LightModel lightSource,
            PlacementType placementType, bool copsEnabled, float offset);

        /// <summary>
        /// Remove one or more placed roadblocks.
        /// </summary>
        /// <param name="removeType">The remove criteria for the roadblocks.</param>
        void RemoveRoadblocks(RemoveType removeType);
    }
}