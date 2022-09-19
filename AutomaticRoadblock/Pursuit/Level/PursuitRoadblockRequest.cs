using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Data;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    internal class PursuitRoadblockRequest
    {
        public RoadblockData RoadblockData { get; internal set; }
        public ERoadblockLevel Level { get; internal set; }
        public Road Road { get; internal set; }
        public Vehicle TargetVehicle { get; internal set; }
        public ERoadblockFlags Flags { get; internal set; }
    }
}