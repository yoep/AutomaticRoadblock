using System;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Vehicles;

namespace AutomaticRoadblocks.ManualPlacement
{
    public interface IManualPlacement : IPreviewSupport, IDisposable
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
        /// The indication if cops should be added to the roadblock.
        /// </summary>
        bool CopsEnabled { get; set; }

        /// <summary>
        /// Set if the speed should be limited around the roadblock.
        /// </summary>
        bool SpeedLimit { get; set; }
        
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