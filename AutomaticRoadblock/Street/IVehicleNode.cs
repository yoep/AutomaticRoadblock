using AutomaticRoadblocks.Preview;
using Rage;

namespace AutomaticRoadblocks.Street
{
    /// <summary>
    /// The basic information of a vehicle node.
    /// This can either be a <see cref="Info.Road"/> or <see cref="Info.Intersection"/> (see <see cref="Type"/> to know which one).
    /// </summary>
    public interface IVehicleNode : IPreviewSupport
    {
        /// <summary>
        /// The center position of the node.
        /// </summary>
        Vector3 Position { get; }
        
        /// <summary>
        /// The heading of the node.
        /// </summary>
        float Heading { get; }
        
        /// <summary>
        /// The actual type of the node.
        /// Based on this type, the interface can be cast to <see cref="Info.Road"/> or <see cref="Info.Intersection"/>.
        /// </summary>
        EStreetType Type { get; }
    }
}