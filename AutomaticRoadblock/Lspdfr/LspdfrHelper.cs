using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Localization;
using LSPD_First_Response;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AutomaticRoadblocks.Lspdfr
{
    public static class LspdfrHelper
    {
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        
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

        /// <summary>
        /// Create a new backup unit with LSPDFR.
        /// This helper function will create plugin instances.
        /// </summary>
        /// <param name="position">The position of the backup unit.</param>
        /// <param name="heading">The heading of the backup vehicle.</param>
        /// <param name="backupUnit">The backup unit type.</param>
        /// <param name="numberOfOccupants">The number of cop peds.</param>
        /// <param name="vehicle">The vehicle that was created.</param>
        /// <param name="cops">The cops that were created.</param>
        /// <param name="recordCollisions">Set if the vehicle collisions should be recorded.</param>
        public static bool CreateBackupUnit(Vector3 position, float heading, EBackupUnit backupUnit, int? numberOfOccupants, out ARVehicle vehicle, out ARPed[] cops, bool recordCollisions = false)
        {
            Assert.True(numberOfOccupants != 0, "numberOfOccupants cannot be 0");
            var instance = Functions.RequestBackup(position, EBackupResponseType.Code3, LspdfrBackupUnit(backupUnit), null, true, true, numberOfOccupants);

            if (instance != null)
            {
                vehicle = new ARVehicle(instance, heading, recordCollisions);
                cops = instance.Occupants
                    .Select(x => new ARPed(x))
                    .ToArray();
                
                return true;
            }

            Logger.Error("Failed to create backup unit, LSPDFR returned null instance");
            vehicle = null;
            cops = null;
            return false;
        }

        /// <summary>
        /// Retrieve the LSPDFR backup unit for the given plugin backup unit.
        /// </summary>
        /// <param name="backupUnit">The plugin backup unit.</param>
        /// <returns>Returns the LSPDFR backup unit.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Is thrown when the backup unit is not supported by LSPDFR.</exception>
        private static EBackupUnitType LspdfrBackupUnit(EBackupUnit backupUnit)
        {
            return backupUnit switch
            {
                EBackupUnit.None => EBackupUnitType.LocalUnit,
                EBackupUnit.LocalPatrol => EBackupUnitType.LocalUnit,
                EBackupUnit.StatePatrol => EBackupUnitType.StateUnit,
                EBackupUnit.Transporter => EBackupUnitType.PrisonerTransport,
                EBackupUnit.LocalSWAT => EBackupUnitType.SwatTeam,
                EBackupUnit.NooseSWAT => EBackupUnitType.NooseTeam,
                _ => throw new ArgumentOutOfRangeException(nameof(backupUnit), backupUnit, "Backup unit not supported")
            };
        }

        public static LocalizationKey ToLocalizationKey(EBackupUnit unit)
        {
            Assert.NotNull(unit, "unit cannot be null");
            return new LocalizationKey("VehicleType" + unit, unit.ToString());
        }
    }
}