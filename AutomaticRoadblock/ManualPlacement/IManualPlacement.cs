using System;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Street.Info;

namespace AutomaticRoadblocks.ManualPlacement
{
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
        void PlaceRoadblock();

        /// <summary>
        /// Remove one or more placed roadblocks.
        /// </summary>
        /// <param name="removeType">The remove criteria for the roadblocks.</param>
        void RemoveRoadblocks(RemoveType removeType);
    }
}