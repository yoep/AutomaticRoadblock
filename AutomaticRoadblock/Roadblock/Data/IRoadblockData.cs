using AutomaticRoadblocks.Data;

namespace AutomaticRoadblocks.Roadblock.Data
{
    public interface IRoadblockData : IDataFile
    {
        /// <summary>
        /// The roadblocks configuration data.
        /// </summary>
        Roadblocks Roadblocks { get; }
    }
}