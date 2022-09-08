using System;
using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Xml;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class LspdfrModelProvider : IModelProvider
    {
        private const string LspdfrDataDirectory = @"./lspdfr/data/";
        private const string AgencyFilename = "agency.xml";
        private const string OutfitsFilename = "outfits.xml";

        private readonly ILogger _logger;
        private readonly ObjectMapper _objectMapper = ObjectMapperFactory.CreateInstance();

        public LspdfrModelProvider(ILogger logger)
        {
            _logger = logger;
        }
        
        #region Properties

        /// <summary>
        /// The agency model data.
        /// </summary>
        public Agencies Agencies { get; private set; }
        
        /// <summary>
        /// The outfits model data.
        /// </summary>
        public Outfits Outfits { get; private set; }

        #endregion

        #region Method

        /// <inheritdoc />
        public void Load()
        {
            Agencies = TryToLoadDatafile<Agencies>(AgencyFilename);
            Outfits = TryToLoadDatafile<Outfits>(OutfitsFilename);
        }

        #endregion

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
           Load();
        }

        private T TryToLoadDatafile<T>(string filename) where T : class
        {
            try
            {
                return _objectMapper.ReadValue<T>(LspdfrDataDirectory + filename);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load {filename} data, {ex.Message}", ex);
            }

            return default;
        }
    }
}