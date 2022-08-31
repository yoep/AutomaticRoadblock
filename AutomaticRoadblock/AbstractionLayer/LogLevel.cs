using System;

namespace AutomaticRoadblocks.AbstractionLayer
{
    [Flags]
    public enum LogLevel
    {
        // 0001 0000
        Trace = 16,

        // 0000 1000
        Debug = 8,

        // 0000 0100
        Info = 4,

        // 0000 0010
        Warn = 2,

        // 0000 0001
        Error = 1,
        
        // 0000 0000
        Disabled = 0
    }
}