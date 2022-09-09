using System;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;

namespace AutomaticRoadblocks.Integrations
{
    public class PluginIntegrationManager : IPluginIntegrationManager
    {
        private readonly ILogger _logger;

        public PluginIntegrationManager(ILogger logger)
        {
            _logger = logger;
        }

        #region Methods
        
        /// <inheritdoc />
        public void LoadPlugins()
        {
            IoC.Instance.GetInstances<IPluginIntegration>()
                .ToList()
                .ForEach(TryPluginInitialization);
        }
        
        #endregion

        #region Functions

        private void TryPluginInitialization(IPluginIntegration plugin)
        {
            try
            {
                plugin.Initialize();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to initialize plugin integration of {plugin.GetType().Name}, {ex.Message}", ex);
            }
        }

        #endregion
    }
}