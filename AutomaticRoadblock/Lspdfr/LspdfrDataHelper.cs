using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Localization;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AutomaticRoadblocks.Lspdfr
{
    public static class LspdfrDataHelper
    {
        private const string DefaultVehicleName = "police";

        private static readonly Random Random = new();
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        private static readonly ILspdfrData LspdfrData = IoC.Instance.GetInstance<ILspdfrData>();
        private static readonly IGame Game = IoC.Instance.GetInstance<IGame>();

        /// <summary>
        /// Retrieve a vehicle model from the LSPDFR config data.
        /// </summary>
        /// <param name="unit">The backup unit type of the vehicle.</param>
        /// <param name="position">The zone position of the vehicle model.</param>
        /// <returns>Returns a vehicle model.</returns>
        public static Model RetrieveVehicleModel(EBackupUnit unit, Vector3 position)
        {
            Assert.NotNull(unit, "unit cannot be null");
            Assert.NotNull(position, "position cannot be null");
            var modelName = DefaultVehicleName;

            try
            {
                var backup = GetBackupForUnitType(unit);
                var agency = LspdfrData.Agencies[backup[Functions.GetZoneAtPosition(position).County]];

                if (agency != null)
                {
                    Logger.Trace($"Found agency {agency} for backup {backup}");
                    var loadout = GetLoadoutForAgency(unit, agency);

                    if (loadout != null)
                    {
                        var vehicles = loadout.Vehicles;

                        Logger.Trace($"Found a total of {vehicles.Count} for loadout {loadout}");
                        modelName = vehicles[Random.Next(vehicles.Count)].ModelName;
                    }
                    else
                    {
                        Logger.Warn($"No loadout available for agency {agency}");
                        Game.DisplayNotificationDebug($"~o~No loadout found for agency {agency.ScriptName}");
                    }
                }
                else
                {
                    Logger.Warn($"No agency found for {backup}, using default vehicle instead");
                    Game.DisplayNotificationDebug($"~o~No agency found for backup unit");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to retrieve vehicle model from LSPDFR data, {ex.Message}", ex);
                Game.DisplayNotificationDebug("~r~LSPDFR data failure, see logs for more info");
            }

            return new Model(modelName);
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
            Assert.NotNull(unit, "unit cannot be null");
            return new LocalizationKey("VehicleType" + unit, unit.ToString());
        }

        private static Backup GetBackupForUnitType(EBackupUnit unit)
        {
            return unit == EBackupUnit.Transporter
                ? LspdfrData.BackupUnits.LocalPatrol
                : LspdfrData.BackupUnits[unit];
        }

        private static Loadout GetLoadoutForAgency(EBackupUnit unit, Agency agency)
        {
            return unit == EBackupUnit.Transporter
                ? agency.Loadout.FirstOrDefault(x => x.Name.Equals(Agency.TransporterLoadout, StringComparison.InvariantCulture))
                : agency.Loadout.FirstOrDefault();
        }
    }
}