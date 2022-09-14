using AutomaticRoadblocks.Data;

namespace AutomaticRoadblocks.Barriers
{
    public interface IBarrierData : IDataFile
    {
        /// <summary>
        /// The barriers model data.
        /// </summary>
        Barriers Barriers { get; }
    }
}