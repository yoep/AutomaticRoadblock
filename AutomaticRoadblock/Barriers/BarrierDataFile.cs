using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Data;
using AutomaticRoadblocks.Logging;

namespace AutomaticRoadblocks.Barriers
{
    public class BarrierDataFile : AbstractDataFileLoader, IBarrierData
    {
        private const string BarriersFilename = "barriers.xml";

        public BarrierDataFile(ILogger logger)
            : base(logger)
        {
        }

        #region Properties

        /// <inheritdoc />
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