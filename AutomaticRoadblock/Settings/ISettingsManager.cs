namespace AutomaticRoadblock.Settings
{
    public interface ISettingsManager
    {
        /// <summary>
        /// Get the general settings for this plugin.
        /// </summary>
        GeneralSettings GeneralSettings { get; }

        /// <summary>
        /// Load the settings from the configuration file.
        /// </summary>
        void Load();
    }
}