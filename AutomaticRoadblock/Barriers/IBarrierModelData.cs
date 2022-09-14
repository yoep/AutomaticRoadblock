using AutomaticRoadblocks.Models;

namespace AutomaticRoadblocks.Barriers
{
    public interface IBarrierModelData : IModelData
    {
        /// <summary>
        /// The barriers model data.
        /// </summary>
        Barriers Barriers { get; }
    }
}