using System;

namespace AutomaticRoadblocks.AbstractionLayer
{
    [Flags]
    public enum LogLevel
    {
        // 0001 1111
        Trace = 31,

        // 0000 1111
        Debug = 15,

        // 0000 0111
        Info = 7,

        // 0000 0011
        Warn = 3,

        // 0000 0001
        Error = 1,
        
        // 0000 0000
        Disabled = 0
    }
}