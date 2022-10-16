using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Data;

namespace AutomaticRoadblocks.Lspdfr
{
    public class LspdfrData : AbstractDataFileLoader, ILspdfrData
    {
        private const string LspdfrDataDirectory = @"./lspdfr/data/";
        private const string AgenciesFilename = "agency.xml";
        private const string BackupUnitsFilename = "backup.xml";
        private const string InventoryFilename = "inventory.xml";
        private const string OutfitFilename = "outfits.xml";

        public LspdfrData(ILogger logger)
            : base(logger, LspdfrDataDirectory)
        {
        }

        #region Properties

        /// <inheritdoc />
        public BackupUnits BackupUnits { get; private set; }

        /// <inheritdoc />
        public Agencies Agencies { get; private set; }

        /// <inheritdoc />
        public Inventories Inventories { get; private set; }

        /// <inheritdoc />
        public Outfits Outfits { get; private set; }

        #endregion

        #region IDataFile

        /// <inheritdoc />
        public override void Reload()
        {
            Logger.Trace($"Loading LSPDFR config data from {DataDirectory}");
            BackupUnits = TryToLoadDatafile(BackupUnitsFilename, BackupUnits.Defaults);
            Agencies = TryToLoadDatafile<Agencies>(AgenciesFilename);
            Inventories = TryToLoadDatafile<Inventories>(InventoryFilename);
            Outfits = TryToLoadDatafile<Outfits>(OutfitFilename);
            Logger.Info("LSPDFR config data has been loaded");
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