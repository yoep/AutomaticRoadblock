using System;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Vehicles;

namespace AutomaticRoadblocks.RedirectTraffic
{
    public interface IRedirectTrafficDispatcher : IPreviewSupport, IDisposable
    {
        /// <summary>
        /// The cone distance of the redirect traffic.
        /// </summary>
        float ConeDistance { get; set; }

        /// <summary>
        /// The vehicle type to use for redirecting the traffic.
        /// </summary>
        VehicleType VehicleType { get; set; }
        
        /// <summary>
        /// The cones which should be used to redirect the traffic.
        /// </summary>
        BarrierType ConeType { get; set; }
        
        /// <summary>
        /// The type of the traffic redirection which should be placed.
        /// </summary>
        RedirectTrafficType Type { get; set; }
        
        /// <summary>
        /// Enable the redirect arrow based on the side of the placement.
        /// </summary>
        bool EnableRedirectionArrow { get; set; }

        /// <summary>
        /// Dispatch a new redirect traffic instance.
        /// </summary>
        void DispatchRedirection();

        /// <summary>
        /// Remove the traffic redirects.
        /// </summary>
        /// <param name="removeType">The removal type.</param>
        void RemoveTrafficRedirects(RemoveType removeType);
    }
}