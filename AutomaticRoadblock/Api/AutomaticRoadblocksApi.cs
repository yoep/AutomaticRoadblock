using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Roadblock;

namespace AutomaticRoadblocks.Api
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class AutomaticRoadblocksApi
    {
        /// <summary>
        /// Get the roadblock dispatcher.
        /// </summary>
        public static IRoadblockDispatcher RoadblockDispatcher => IoC.Instance.GetInstance<IRoadblockDispatcher>();
    }
}