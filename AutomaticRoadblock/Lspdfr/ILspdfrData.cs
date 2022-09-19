using AutomaticRoadblocks.Data;

namespace AutomaticRoadblocks.Lspdfr
{
    public interface ILspdfrData : IDataFile
    {
        /// <summary>
        /// The backup unit config from LSPDFR.
        /// </summary>
        BackupUnits BackupUnits { get; }
        
        /// <summary>
        /// The LSPDFR agency config data.
        /// </summary>
        Agencies Agencies { get; }
    }
}