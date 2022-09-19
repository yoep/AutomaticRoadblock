using System;
using System.IO;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Xml;

namespace AutomaticRoadblocks.Data
{
    /// <summary>
    /// Abstract loader implementation of <see cref="IDataFile"/>.
    /// </summary>
    public abstract class AbstractDataFileLoader : IDataFile
    {
        private const string DefaultDataDirectory = @"./plugins/LSPDFR/Automatic Roadblocks/data/";
        
        protected readonly ILogger Logger;
        protected readonly ObjectMapper ObjectMapper = ObjectMapperFactory.CreateInstance();
        protected readonly string DataDirectory;

        protected AbstractDataFileLoader(ILogger logger)
        {
            Logger = logger;
            DataDirectory = DefaultDataDirectory;
        }

        protected AbstractDataFileLoader(ILogger logger, string dataDirectory)
        {
            Logger = logger;
            DataDirectory = dataDirectory;
        }

        /// <inheritdoc />
        public abstract void Reload();

        protected T TryToLoadDatafile<T>(string filename, T defaultValue = null) where T : class
        {
            var fullFilePath = Path.Combine(DataDirectory, filename);

            if (!File.Exists(fullFilePath))
            {
                Logger.Warn($"Data file {fullFilePath} doesn't exist, using defaults instead");
                return defaultValue;
            }

            try
            {
                Logger.Trace($"Loading {fullFilePath} data file");
                return ObjectMapper.ReadValue<T>(fullFilePath);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load {fullFilePath} data file, {ex.Message}", ex);
            }

            return defaultValue;
        }
    }
}