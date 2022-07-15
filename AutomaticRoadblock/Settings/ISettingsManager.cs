namespace AutomaticRoadblocks.Settings
{
    public interface ISettingsManager
    {
        /// <summary>
        /// Get the general settings for this plugin.
        /// </summary>
        GeneralSettings GeneralSettings { get; }
        
        /// <summary>
        /// Get the automatic roadblocks settings for this plugin.
        /// </summary>
        AutomaticRoadblocksSettings AutomaticRoadblocksSettings { get; }

        /// <summary>
        /// Get the manual placement settings.
        /// </summary>
        ManualPlacementSettings ManualPlacementSettings { get; }

        /// <summary>
        /// Load the settings from the configuration file.
        /// </summary>
        void Load();
    }
}