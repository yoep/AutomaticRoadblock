using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Xml;
using LSPD_First_Response.Engine.Scripting;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class LspdfrModelData : IModelData
    {
        private const string LspdfrDataDirectory = @"./lspdfr/data/";
        private const string AgencyFilename = "agency.xml";
        private const string OutfitsFilename = "outfits.xml";
        private const string BackupFilename = "backup.xml";

        private readonly ILogger _logger;
        private readonly ObjectMapper _objectMapper = ObjectMapperFactory.CreateInstance();
        private readonly Random _random = new();

        public LspdfrModelData(ILogger logger)
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

        /// <summary>
        /// The backup units for each region.
        /// </summary>
        public BackupUnits BackupUnits { get; private set; }

        /// <inheritdoc />
        public PedModelInfo Ped(EUnitType unitType, EWorldZoneCounty zone)
        {
            return RetrieveModelInfoFor(unitType, zone);
        }

        /// <inheritdoc />
        public VehicleModelInfo Vehicle(EUnitType unitType, EWorldZoneCounty zone)
        {
            return null;
        }

        #endregion

        #region Method

        /// <inheritdoc />
        public void Reload()
        {
            Agencies = TryToLoadDatafile<Agencies>(AgencyFilename);
            Outfits = TryToLoadDatafile<Outfits>(OutfitsFilename);
            BackupUnits = TryToLoadDatafile(BackupFilename, BackupUnits.Default);
        }

        #endregion

        #region Functions

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            Reload();
        }

        private T TryToLoadDatafile<T>(string filename, T defaultValue = null) where T : class
        {
            try
            {
                return _objectMapper.ReadValue<T>(LspdfrDataDirectory + filename);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load {filename} data, {ex.Message}", ex);
            }

            return defaultValue;
        }

        private PedModelInfo RetrieveModelInfoFor(EUnitType unitType, EWorldZoneCounty zone)
        {
            // retrieve the backup info for the given unit type
            var backup = GetBackupFor(unitType);
            // retrieve a agency info based on the backup zone
            var agency = GetAgencyFor(ZoneToDataString(zone), backup);
            var ped = SelectPedBasedOmChance(agency.Loadout.Peds);

            return null;
        }

        private Ped SelectPedBasedOmChance(List<Ped> peds)
        {
            var threshold = _random.Next(101);
            var totalChance = 0;

            foreach (var ped in peds.OrderBy(x => x.Chance))
            {
                if (totalChance < threshold && threshold <= ped.Chance + totalChance)
                {
                    return ped;
                }

                totalChance += ped.Chance;
            }

            return peds.Last();
        }

        private Agency GetAgencyFor(string zone, Backup backup)
        {
            var agencies = backup[zone];
            var agencyName = agencies[_random.Next(agencies.Count)].Name;

            return Agencies.Items
                .First(x => x.ScriptName.Equals(agencyName));
        }

        private Backup GetBackupFor(EUnitType unitType)
        {
            return unitType switch
            {
                EUnitType.LocalPatrol => BackupUnits.LocalPatrol,
                EUnitType.StatePatrol => BackupUnits.StatePatrol,
                EUnitType.LocalSwat => BackupUnits.LocalSWAT,
                EUnitType.NooseSwat => BackupUnits.NooseSWAT,
                _ => throw new NotSupportedException($"Unit type {unitType} is not supported within the model data")
            };
        }

        private static string ZoneToDataString(EWorldZoneCounty zone)
        {
            return zone switch
            {
                EWorldZoneCounty.LosSantos => "LosSantosCity",
                EWorldZoneCounty.LosSantosCounty => "LosSantosCounty",
                EWorldZoneCounty.BlaineCounty => "BlaineCounty",
                EWorldZoneCounty.NorthYankton => "NorthYankton",
                EWorldZoneCounty.CayoPerico => "CayoPerico",
                _ => throw new NotSupportedException($"The zone {zone} is not supported within the model data")
            };
        }

        #endregion
    }
}