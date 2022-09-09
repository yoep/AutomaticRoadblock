namespace AutomaticRoadblocks.Integrations
{
    public interface IPluginIntegrationManager
    {
        /// <summary>
        /// Load all the registered plugin integrations.
        /// </summary>
        void LoadPlugins();
    }
}