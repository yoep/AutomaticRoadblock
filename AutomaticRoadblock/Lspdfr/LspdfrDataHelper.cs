using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Localization;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AutomaticRoadblocks.Lspdfr
{
    public static class LspdfrDataHelper
    {
        private static readonly Random Random = new();
        private static readonly ILspdfrData LspdfrData = IoC.Instance.GetInstance<ILspdfrData>();

        /// <summary>
        /// Retrieve a vehicle model from the LSPDFR config data.
        /// </summary>
        /// <param name="unit">The backup unit type of the vehicle.</param>
        /// <param name="position">The zone position of the vehicle model.</param>
        /// <returns>Returns a vehicle model.</returns>
        public static Model RetrieveVehicleModel(EBackupUnit unit, Vector3 position)
        {
            var backup = unit == EBackupUnit.Transporter ? LspdfrData.BackupUnits.LocalPatrol : LspdfrData.BackupUnits[unit];
            var agency = LspdfrData.Agencies[backup[Functions.GetZoneAtPosition(position).County]];
            var vehicles = unit == EBackupUnit.Transporter
                ? agency.Loadout.First(x => x.Name.Equals(Agency.TransporterLoadout, StringComparison.InvariantCulture)).Vehicles
                : agency.Loadout.First().Vehicles;

            return new Model(vehicles[Random.Next(vehicles.Count)].ModelName);
        }

        /// <summary>
        /// The available backup units.
        /// </summary>
        /// <returns>Returns the available units.</returns>
        public static IEnumerable<EBackupUnit> BackupUnits()
        {
            return new[]
            {
                EBackupUnit.None,
                EBackupUnit.LocalPatrol,
                EBackupUnit.StatePatrol,
                EBackupUnit.Transporter,
                EBackupUnit.LocalSWAT,
                EBackupUnit.NooseSWAT
            };
        }

        public static LocalizationKey ToLocalizationKey(EBackupUnit unit)
        {
            return new LocalizationKey("VehicleType" + unit, unit.ToString());
        }
    }
}