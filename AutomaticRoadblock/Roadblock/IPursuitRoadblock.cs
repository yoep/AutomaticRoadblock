using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    /// <summary>
    /// An extension on top of the normal <see cref="IRoadblock"/> which has additional features for pursuits.
    /// </summary>
    public interface IPursuitRoadblock : IRoadblock
    {
        /// <summary>
        /// The target vehicle this roadblock is trying to stop.
        /// </summary>
        Vehicle TargetVehicle { get; }
        
        /// <summary>
        /// Invoked when a cop from this roadblock is killed.
        /// </summary>
        event RoadblockEvents.RoadblockCopKilled RoadblockCopKilled;
        
        /// <summary>
        /// Invoked when cops from this roadblock are joining the pursuit.
        /// </summary>
        event RoadblockEvents.RoadblockCopsJoiningPursuit RoadblockCopsJoiningPursuit;
    }
}