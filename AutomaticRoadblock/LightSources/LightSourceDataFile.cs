using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Data;
using AutomaticRoadblocks.Logging;

namespace AutomaticRoadblocks.LightSources
{
    public class LightSourceDataFile : AbstractDataFileLoader, ILightSourceData
    {
        private const string LightsFilename = "lights.xml";

        public LightSourceDataFile(ILogger logger) 
            : base(logger)
        {
        }
        
        #region Properties

        /// <inheritdoc />
        public Lights Lights { get; private set; }

        #endregion
        
        #region Method

        /// <inheritdoc />
        public override void Reload()
        {
            Lights = TryToLoadDatafile(LightsFilename, Lights.Defaults);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Lights)}: {Lights}";
        }

        #endregion
        
        #region Functions

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            Reload();
        }

        #endregion
    }
}