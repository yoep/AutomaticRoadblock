using AutomaticRoadblocks.Preview;
using Rage;

namespace AutomaticRoadblocks.Street
{
    /// <summary>
    /// The information of a vehicle node presented as street info.
    /// This can either be a <see cref="Info.Road"/> or <see cref="Info.Intersection"/> (see <see cref="Type"/> to know which one).
    /// </summary>
    public interface IStreet : IPreviewSupport
    {
        /// <summary>
        /// The center position of the street.
        /// </summary>
        Vector3 Position { get; }
        
        /// <summary>
        /// The heading of the street which is based on the vehicle node that was used.
        /// </summary>
        float Heading { get; }
        
        /// <summary>
        /// The type of the street.
        /// </summary>
        EStreetType Type { get; }
    }
}