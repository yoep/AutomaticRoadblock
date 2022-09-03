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

        private readonly ILogger _logger;

        public LspdfrModelProvider(ILogger logger)
        {
            _logger = logger;
        }
        
        #region Properties

        /// <inheritdoc />
        public Agencies Agencies { get; private set; }

        #endregion

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            var objectMapper = ObjectMapperFactory.CreateInstance();

            try
            {
                Agencies = objectMapper.ReadValue<Agencies>(LspdfrDataDirectory + AgencyFilename);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load agency.xml data, {ex.Message}", ex);
            }
        }
    }
}