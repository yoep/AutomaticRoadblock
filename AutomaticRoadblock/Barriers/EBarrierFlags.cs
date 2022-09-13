using System;

namespace AutomaticRoadblocks.Barriers
{
    [Flags]
    public enum EBarrierFlags
    {
        None = 0,
        ManualPlacement = 1,
        RedirectTraffic = 2
    }
}