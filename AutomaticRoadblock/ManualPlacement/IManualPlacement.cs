using System;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Utils.Road;

namespace AutomaticRoadblocks.ManualPlacement
{
    public interface IManualPlacement : IDisposable
    {
        /// <summary>
        /// The barrier type to use within the roadblock.
        /// </summary>
        BarrierType Barrier { get; set; }
        
        /// <summary>
        /// The vehicle type to use within the roadblock.
        /// </summary>
        VehicleType VehicleType { get; set; }
        
        /// <summary>
        /// The light source type to use within the roadblock.
        /// </summary>
        LightSourceType LightSourceType { get; set; }
        
        /// <summary>
        /// The placement type of the roadblock.
        /// </summary>
        PlacementType PlacementType { get; set; }
        
        /// <summary>
        /// Indication if flares should be added to the roadblock.
        /// </summary>
        bool FlaresEnabled { get; set; }
        
        /// <summary>
        /// Determine the placement location based on the current player location.
        /// </summary>
        /// <returns>Returns the road for the placement.</returns>
        Road DetermineLocation();

        /// <summary>
        /// Create a preview of the roadblock that will be placed. 
        /// </summary>
        /// <param name="force">Force the recreation of the preview.</param>
        void CreatePreview(bool force = false);

        /// <summary>
        /// Remove all previews of the roadblock that might be placed.
        /// </summary>
        void RemovePreviews();

        /// <summary>
        /// Place a roadblock based on the <see cref="DetermineLocation"/> <see cref="Road"/>.
        /// </summary>
        void PlaceRoadblock();
    }
}