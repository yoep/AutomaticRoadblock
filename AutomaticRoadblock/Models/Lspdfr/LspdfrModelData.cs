using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using LSPD_First_Response.Engine.Scripting;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class LspdfrModelData : AbstractModelDataLoader, ILspdfrModelData
    {
        private const string LspdfrDataDirectory = @"./lspdfr/data/";
        private const string AgencyFilename = "agency.xml";
        private const string OutfitsFilename = "outfits.xml";
        private const string BackupFilename = "backup.xml";
        private const string InventoryFilename = "inventory.xml";

        private static readonly Random Random = new();

        public LspdfrModelData(ILogger logger)
            : base(logger, LspdfrDataDirectory)
        {
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

        /// <summary>
        /// The inventories with their items.
        /// </summary>
        public Inventories Inventories { get; private set; }

        /// <inheritdoc />
        public PedModelInfo Ped(EUnitType unitType, EWorldZoneCounty zone)
        {
            return RetrievePedModelInfoFor(unitType, zone);
        }

        /// <inheritdoc />
        public VehicleModelInfo Vehicle(EUnitType unitType, EWorldZoneCounty zone)
        {
            return RetrieveVehicleModelInfoFor(unitType, zone);
        }

        #endregion

        #region Method

        /// <inheritdoc />
        public override void Reload()
        {
            Agencies = TryToLoadDatafile<Agencies>(AgencyFilename);
            Outfits = TryToLoadDatafile<Outfits>(OutfitsFilename);
            BackupUnits = TryToLoadDatafile(BackupFilename, BackupUnits.Default);
            Inventories = TryToLoadDatafile<Inventories>(InventoryFilename);
        }

        #endregion

        #region Functions

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            Reload();
        }

        private PedModelInfo RetrievePedModelInfoFor(EUnitType unitType, EWorldZoneCounty zone)
        {
            var agency = RetrieveAgencyFor(unitType, zone);
            var ped = SelectPedBasedOnChance(agency.Loadout.Peds);

            return new PedModelInfo
            {
                Name = ped.Name
            };
        }

        private VehicleModelInfo RetrieveVehicleModelInfoFor(EUnitType unitType, EWorldZoneCounty zone)
        {
            var agency = RetrieveAgencyFor(unitType, zone);
            var vehicles = agency.Loadout.Vehicles;

            // verify if any vehicle is configured for the criteria
            if (vehicles?.Any() != true)
            {
                Logger.Warn($"No vehicle model available for agency {agency}");
                throw new NoModelAvailableException(unitType, zone);
            }

            var vehicle = vehicles[Random.Next(vehicles.Count)];
            return new VehicleModelInfo
            {
                Name = vehicle.Name
            };
        }

        private Agency RetrieveAgencyFor(EUnitType unitType, EWorldZoneCounty zone)
        {
            // retrieve the backup info for the given unit type
            var backup = GetBackupFor(unitType);
            // retrieve a agency info based on the backup zone
            return GetAgencyFor(ZoneToDataString(zone), backup);
        }

        private Ped SelectPedBasedOnChance(List<Ped> peds)
        {
            var threshold = Random.Next(101);
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
            var agencyName = agencies[Random.Next(agencies.Count)].Name;

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