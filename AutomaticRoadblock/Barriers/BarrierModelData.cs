using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Models;

namespace AutomaticRoadblocks.Barriers
{
    public class BarrierModelData : AbstractModelDataLoader, IBarrierModelData
    {
        private const string ModelDataDirectory = @"./plugins/LSPDFR/Automatic Roadblocks/data/";
        private const string BarriersFilename = "barriers.xml";

        public BarrierModelData(ILogger logger)
            : base(logger, ModelDataDirectory)
        {
        }

        #region Properties
        
        public Barriers Barriers { get; private set; }

        #endregion

        #region Method

        /// <inheritdoc />
        public override void Reload()
        {
            Barriers = TryToLoadDatafile(BarriersFilename, Barriers.Defaults);
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