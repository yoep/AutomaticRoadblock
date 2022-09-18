using System;

namespace AutomaticRoadblocks.LightSources
{
    [Flags]
    public enum ELightSourceFlags
    {
        None = 0,
        Lane = 1,
        RoadLeft = 2,
        RoadRight = 4
    }
}