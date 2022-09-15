using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Data;

namespace AutomaticRoadblocks.Roadblock.Data
{
    public class RoadblockDataFile : AbstractDataFileLoader, IRoadblockData
    {
        private const string RoadblockFilename = "roadblocks.xml";
        
        public RoadblockDataFile(ILogger logger) 
            : base(logger)
        {
        }

        #region Properties

        /// <inheritdoc />
        public Roadblocks Roadblocks { get; private set; }

        #endregion
        
        #region Methods

        /// <inheritdoc />
        public override void Reload()
        {
            Roadblocks = TryToLoadDatafile(RoadblockFilename, Roadblocks.Defaults);
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