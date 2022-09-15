namespace AutomaticRoadblocks.Data
{
    /// <summary>
    /// Defines the basic functionality of a datafile which is used by the plugin's customizations.
    /// </summary>
    public interface IDataFile
    {
        /// <summary>
        /// Reload the data file information from the file system.
        /// </summary>
        void Reload();
    }
}