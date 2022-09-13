using System;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Xml;

namespace AutomaticRoadblocks.Models
{
    /// <summary>
    /// Abstract loader implementation of <see cref="IModelData"/>.
    /// </summary>
    public abstract class AbstractModelDataLoader : IModelData
    {
        protected readonly ILogger Logger;
        protected readonly ObjectMapper ObjectMapper = ObjectMapperFactory.CreateInstance();
        
        private readonly string _dataDirectory;

        protected AbstractModelDataLoader(ILogger logger, string dataDirectory)
        {
            Logger = logger;
            _dataDirectory = dataDirectory;
        }

        /// <inheritdoc />
        public abstract void Reload();

        protected T TryToLoadDatafile<T>(string filename, T defaultValue = null) where T : class
        {
            try
            {
                return ObjectMapper.ReadValue<T>(_dataDirectory + filename);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load {filename} data, {ex.Message}", ex);
            }

            return defaultValue;
        }
    }
}