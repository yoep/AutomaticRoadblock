using System;
using System.Collections.Generic;
using AutomaticRoadblocks.Pursuit.Level;
using AutomaticRoadblocks.Roadblock;

namespace AutomaticRoadblocks.Pursuit.Factory
{
    internal static class PursuitRoadblockFactory
    {
        private static readonly Dictionary<ERoadblockLevel, Func<PursuitRoadblockRequest, IPursuitRoadblock>> Roadblocks = new()
        {
            {
                ERoadblockLevel.Level1, request => new PursuitRoadblockLevel1(request)
            },
            {
                ERoadblockLevel.Level2, request => new PursuitRoadblockLevel2(request)
            },
            {
                ERoadblockLevel.Level3, request => new PursuitRoadblockLevel3(request)
            },
            {
                ERoadblockLevel.Level4, request => new PursuitRoadblockLevel4(request)
            },
            {
                ERoadblockLevel.Level5, request => new PursuitRoadblockLevel5(request)
            }
        };

        internal static IPursuitRoadblock Create(PursuitRoadblockRequest request)
        {
            Assert.NotNull(request, "request cannot be null");
            Assert.NotNull(request.RoadblockData, $"request is invalid, {nameof(request.RoadblockData)} cannot be null");
            Assert.NotNull(request.Level, $"request is invalid, {nameof(request.Level)} cannot be null");
            Assert.NotNull(request.Road, $"request is invalid, {nameof(request.Road)} cannot be null");
            Assert.NotNull(request.TargetVehicle, $"request is invalid, {nameof(request.TargetVehicle)} cannot be null");
            Assert.NotNull(request.Flags, $"request is invalid, {nameof(request.Flags)} cannot be null");
            return Roadblocks[request.Level].Invoke(request);
        }
    }
}