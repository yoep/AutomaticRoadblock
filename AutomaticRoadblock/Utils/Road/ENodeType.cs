using System;

namespace AutomaticRoadblocks.Utils.Road
{
    /// <summary>
    /// The node type flags.
    /// </summary>
    /// <remarks>Info: https://gta.fandom.com/wiki/Paths_(GTA_V)#Analysis</remarks>
    [Flags]
    public enum ENodeType
    {
        // 0000 0001
        IsWaterNode = 1,

        // 0000 0010
        Unused = 2,

        // 0000 0100
        IsFreeway = 4,

        // 0000 1000
        IsGravelRoad = 8,

        // 0001 0000
        IsBackroad = 16,

        // 0010 0000
        IsOnWater = 32,

        // 0100 0000
        IsPedCrossway = 64,

        // 1000 0000
        TrafficLightExists = 128,
        LeftTurnNoReturn = 256,
        RightTurnNoReturn = 512,

        // 0000 0001 0000 0000 0000
        IsOffRoad = 4096,

        // 0000 0010 0000 0000 0000
        NoRightTurn = 8192,

        // 0000 0100 0000 0000 0000
        NoBigVehicles = 16384,

        // 0000 1000 0000 0000 0000
        IsKeepLeft = 32768,

        // 0001 0000 0000 0000 0000
        IsKeepRight = 65536,

        // 0010 0000 0000 0000 0000
        IsSLipLane = 131072
    }
}