using AutomaticRoadblocks.ManualPlacement;
using AutomaticRoadblocks.Vehicles;

namespace AutomaticRoadblocks.RedirectTraffic
{
    public interface IRedirectTrafficDispatcher
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
        /// Dispatch a new redirect traffic instance.
        /// </summary>
        void Dispatch();

        /// <summary>
        /// Create a new preview of the redirect traffic instance.
        /// </summary>
        /// <param name="force">Force a redraw of the preview.</param>
        void CreatePreview(bool force = false);

        /// <summary>
        /// Remove all active redirect traffic previews.
        /// </summary>
        void RemovePreviews();
    }
}