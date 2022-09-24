using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Data;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Utils;
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
                var loadout = RetrieveLoadout(unit, position);
                Logger.Trace($"Retrieved loadout {loadout.Name} for unit type {unit} at position {position}");
                var vehicles = loadout.Vehicles;

                Logger.Trace($"Found a total of {vehicles.Count} for loadout {loadout}");
                modelName = vehicles[Random.Next(vehicles.Count)].ModelName;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to retrieve vehicle model from LSPDFR data, {ex.Message}", ex);
                Game.DisplayNotificationDebug("~r~LSPDFR data failure, see logs for more info");
            }

            return new Model(modelName);
        }

        /// <summary>
        /// Retrieve a cop ped entity for the given unit type and position.
        /// </summary>
        /// <param name="unit">The unit type to create.</param>
        /// <param name="position">the position of the entity.</param>
        /// <returns>Returns the </returns>
        public static Ped RetrieveCop(EBackupUnit unit, Vector3 position)
        {
            var loadout = RetrieveLoadout(unit, position);
            var pedData = ChanceProvider.Retrieve(loadout.Peds);
            var ped = new Ped(pedData.ModelName, GameUtils.GetOnTheGroundPosition(position), 0f);

            if (pedData.IsInventoryAvailable)
                DoInternalInventoryCreation(ped, pedData);

            if (pedData.Helmet)
                ped.GiveHelmet(false, HelmetTypes.PoliceMotorcycleHelmet, 0);

            // always give the ped a flashlight
            ped.Inventory.GiveFlashlight();

            return ped;
        }

        /// <summary>
        /// Retrieve the load for the given unit and position.
        /// </summary>
        /// <param name="unit">The backup unit type.</param>
        /// <param name="position">The position for the agency.</param>
        /// <returns>Returns the loadout for the unit type at the given location.</returns>
        public static Loadout RetrieveLoadout(EBackupUnit unit, Vector3 position)
        {
            Assert.NotNull(unit, "unit cannot be null");
            Assert.NotNull(position, "position cannot be null");
            var backup = GetBackupForUnitType(unit);
            var agency = GetAgency(backup, position);

            return GetLoadoutForAgency(unit, agency);
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

        private static Agency GetAgency(Backup backup, Vector3 position)
        {
            return LspdfrData.Agencies[backup[Functions.GetZoneAtPosition(position).County]];
        }

        private static Loadout GetLoadoutForAgency(EBackupUnit unit, Agency agency)
        {
            return unit == EBackupUnit.Transporter
                ? agency.Loadout.FirstOrDefault(x => x.Name.Equals(Agency.TransporterLoadout, StringComparison.InvariantCulture))
                : agency.Loadout.FirstOrDefault();
        }

        private static void DoInternalInventoryCreation(Ped ped, PedData pedData)
        {
            var inventoryData = LspdfrData.Inventories[pedData.Inventory];
            var weaponData = ChanceProvider.Retrieve(inventoryData.Weapon);

            if (inventoryData.IsStunWeaponAvailable)
                ped.Inventory.GiveNewWeapon(new WeaponAsset(inventoryData.StunWeapon.AssetName), -1, false);

            if (weaponData != null)
            {
                var weaponAsset = new WeaponAsset(weaponData.AssetName);
                ped.Inventory.GiveNewWeapon(weaponAsset, -1, true);

                foreach (var component in weaponData.Component)
                {
                    ped.Inventory.AddComponentToWeapon(weaponAsset, component);
                }
            }

            ped.Armor = inventoryData.Armor;
        }
    }
}