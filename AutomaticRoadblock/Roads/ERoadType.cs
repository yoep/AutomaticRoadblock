using System.Diagnostics.CodeAnalysis;

namespace AutomaticRoadblocks.Roads
{
    /// <summary>
    /// Defines the road types on which can be searched.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum ERoadType
    {
        All = 0,
        MajorRoads = 1,
        MajorRoadsNoJunction = 2
    }
}