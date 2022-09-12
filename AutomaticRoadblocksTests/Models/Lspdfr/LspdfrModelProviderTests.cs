using System.Collections.Generic;
using System.Linq;
using LSPD_First_Response.Engine.Scripting;
using Xunit;
using Xunit.Abstractions;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class LspdfrModelProviderTests
    {
        public LspdfrModelProviderTests(ITestOutputHelper testOutputHelper)
        {
            TestUtils.InitializeIoC();
            TestUtils.SetLogger(testOutputHelper);
        }

        [Fact]
        public void TestAgenciesDeserialization()
        {
            var provider = (LspdfrModelData)IoC.Instance.GetInstance<IModelData>();

            Xunit.Assert.NotNull(provider.Agencies);
        }

        [Fact]
        public void TestOutfitsDeserialization()
        {
            var provider = (LspdfrModelData)IoC.Instance.GetInstance<IModelData>();

            Xunit.Assert.NotNull(provider.Outfits);
        }

        [Fact]
        public void TestInventoryDeserialization()
        {
            var provider = (LspdfrModelData)IoC.Instance.GetInstance<IModelData>();

            Xunit.Assert.NotNull(provider.Inventories);
        }

        [Fact]
        public void TestAgencyDeserialization()
        {
            var expectedResult = new Agency("Los Santos Police Department", "LSPD", "lspd", "default", "web_lossantospolicedept", "web_lossantospolicedept",
                new Loadout("Patrol Unit", new List<Vehicle>
                {
                    new Vehicle("police"),
                    new Vehicle("police2"),
                    new Vehicle("police3"),
                }, new List<Ped>
                {
                    new Ped("mp_m_freemode_01", 60, "lspd_cop.m_base", "patrol"),
                    new Ped("mp_m_freemode_01", 10, "lspd_cop", "patrol_stun"),
                    new Ped("mp_f_freemode_01", 25, "lspd_cop.f_base", "patrol"),
                    new Ped("mp_f_freemode_01", 5, "lspd_cop", "patrol_stun"),
                }, new NumPeds(2, 2)));
            var provider = (LspdfrModelData)IoC.Instance.GetInstance<IModelData>();

            var result = provider.Agencies;

            Xunit.Assert.NotNull(provider.Agencies);
            var agency = result.Items.First(x => x.Name.Equals("Los Santos Police Department"));
            Xunit.Assert.Equal(expectedResult, agency);
        }

        [Fact]
        public void TestBackupUnitsDeserialization()
        {
            const string agencyLocalPatrolCity = "lspd";
            const string agencyLocalPatrolCounty = "lssd";
            const string agencyLocalPatrolYankton = "nysp";
            var provider = (LspdfrModelData)IoC.Instance.GetInstance<IModelData>();

            Xunit.Assert.NotNull(provider.BackupUnits);
            Xunit.Assert.NotNull(provider.BackupUnits.LocalPatrol);
            Xunit.Assert.NotNull(provider.BackupUnits.StatePatrol);
            Xunit.Assert.NotNull(provider.BackupUnits.LocalSWAT);
            Xunit.Assert.Equal(agencyLocalPatrolCity, provider.BackupUnits.LocalPatrol.LosSantosCity[0].Name);
            Xunit.Assert.Equal(agencyLocalPatrolCounty, provider.BackupUnits.LocalPatrol.LosSantosCounty[0].Name);
            Xunit.Assert.Equal(agencyLocalPatrolCounty, provider.BackupUnits.LocalPatrol.BlaineCounty[0].Name);
            Xunit.Assert.Equal(agencyLocalPatrolYankton, provider.BackupUnits.LocalPatrol.NorthYankton[0].Name);
        }

        [Fact]
        public void TestRetrievePedModelInfoForLocalPatrolCity()
        {
            var provider = IoC.Instance.GetInstance<IModelData>();
            var expectedModelNames = new List<string>
            {
                "mp_m_freemode_01",
                "mp_f_freemode_01"
            };

            var result = provider.Ped(EUnitType.LocalPatrol, EWorldZoneCounty.LosSantos);

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Contains(result.Name, expectedModelNames);
        }

        [Fact]
        public void TestRetrieveVehicleModelInfoForLocalPatrolCity()
        {
            var provider = IoC.Instance.GetInstance<IModelData>();
            var expectedVehicleNames = new List<string>
            {
                "police",
                "police2",
                "police3",
            };

            var result = provider.Vehicle(EUnitType.LocalPatrol, EWorldZoneCounty.LosSantos);

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Contains(result.Name, expectedVehicleNames);
        }

        [Fact]
        public void TestInventoryMultipleWeaponsDeserialization()
        {
            var provider = (LspdfrModelData)IoC.Instance.GetInstance<IModelData>();
            var expectedResult = new Inventory("Patrol", "patrol", new List<InventoryWeapon>
            {
                new InventoryWeapon("WEAPON_PUMPSHOTGUN", 10),
                new InventoryWeapon("WEAPON_COMBATPISTOL", 45),
                new InventoryWeapon("WEAPON_PISTOL", 45)
            }, new StunWeapon("WEAPON_STUNGUN", 100));

            var result = provider.Inventories;

            Xunit.Assert.NotNull(result);
            var patrolInventory = result.Items.First(x => x.Name.Equals("Patrol"));
            Xunit.Assert.Equal(expectedResult, patrolInventory);
        }

        [Fact]
        public void TestInventorySingleWeaponDeserialization()
        {
            var provider = (LspdfrModelData)IoC.Instance.GetInstance<IModelData>();
            var expectedResult = new Inventory("Default", "default", new List<InventoryWeapon>
            {
                new InventoryWeapon("WEAPON_PISTOL", 100),
            }, null);

            var result = provider.Inventories;

            Xunit.Assert.NotNull(result);
            var inventory = result.Items.First(x => x.Name.Equals("Default"));
            Xunit.Assert.Equal(expectedResult, inventory);
        }

        [Fact]
        public void TestInventoryWeaponComponentsDeserialization()
        {
            var provider = (LspdfrModelData)IoC.Instance.GetInstance<IModelData>();
            var expectedResult = new Inventory("SWAT", "swat", new List<InventoryWeapon>
            {
                new InventoryWeapon("WEAPON_CARBINERIFLE", new List<string>
                {
                    "COMPONENT_AT_AR_FLSH",
                    "COMPONENT_AT_AR_AFGRIP",
                    "COMPONENT_AT_SCOPE_MEDIUM",
                }, 70),
                new InventoryWeapon("WEAPON_PUMPSHOTGUN", new List<string>
                {
                    "COMPONENT_AT_AR_FLSH",
                }, 15),
                new InventoryWeapon("WEAPON_COMBATPISTOL", new List<string>
                {
                    "COMPONENT_AT_PI_FLSH",
                }, 15),
            }, new StunWeapon("WEAPON_STUNGUN", 100));

            var result = provider.Inventories;

            Xunit.Assert.NotNull(result);
            var inventory = result.Items.First(x => x.Name.Equals("SWAT"));
            Xunit.Assert.Equal(expectedResult, inventory);
        }
    }
}