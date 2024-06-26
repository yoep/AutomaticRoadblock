using System;

namespace AutomaticRoadblocks.Street
{
    /// <summary>
    /// The node type flags.
    /// </summary>
    /// <remarks>Info: https://gta.fandom.com/wiki/Paths_(GTA_V)#Analysis</remarks>
    [Flags]
    public enum ENodeFlag
    {
        None = 0,
        
        // 0000 0001
        // not sure for what this bit stands 
        Unknown = 1,

        // 0000 0010
        Unused = 2,

        // 0000 0100
        IsAlley = 4,

        // 0000 1000
        IsGravelRoad = 8,

        // 0001 0000
        IsBackroad = 16,

        // 0010 0000
        IsOnWater = 32,

        // 0100 0000
        IsPedCrossway = 64,

        // 1000 0000
        IsJunction = 128,
        
        // 0001 0000 0000
        LeftTurnNoReturn = 256,
        
        // 0010 0000 0000
        RightTurnNoReturn = 512,

        // 0000 0001 0000 0000 0000
        IsOffRoad = 4096,

        // 0000 0010 0000 0000 0000
        NoRightTurn = 8192,

        // 0000 0100 0000 0000 0000
        NoBigVehicles = 16384,
    }
}