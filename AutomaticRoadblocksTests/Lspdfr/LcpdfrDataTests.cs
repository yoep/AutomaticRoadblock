using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AutomaticRoadblocks.Lspdfr
{
    public class LcpdfrDataTests
    {
        public LcpdfrDataTests(ITestOutputHelper testOutputHelper)
        {
            TestUtils.InitializeIoC();
            TestUtils.SetLogger(testOutputHelper);
        }

        [Fact]
        public void TestBackupUnitsDeserialization()
        {
            var localPatrol = new Backup("lspd", "lssd", "lssd", "nysp", "sapr", "sapr");
            var localSwat = new Backup("lspd_swat", "lssd_swat", "lssd_swat", "nysp", "sapr");
            var data = IoC.Instance.GetInstance<ILspdfrData>();

            var result = data.BackupUnits;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(localPatrol, result.LocalPatrol);
            Xunit.Assert.Equal(localSwat, result.LocalSWAT);
        }

        [Fact]
        public void TestAgenciesDeserialization()
        {
            var lspdAgency = new Agency("Los Santos Police Department", "LSPD", "lspd", "default", "web_lossantospolicedept", "web_lossantospolicedept",
                new List<Loadout>
                {
                    new Loadout("Patrol Unit", new List<VehicleData>
                    {
                        new VehicleData("police"),
                        new VehicleData("police2"),
                        new VehicleData("police3"),
                    }, new List<PedData>
                    {
                        new PedData("mp_m_freemode_01", 60, "lspd_cop.m_base", "patrol"),
                        new PedData("mp_m_freemode_01", 10, "lspd_cop", "patrol_stun"),
                        new PedData("mp_f_freemode_01", 25, "lspd_cop.f_base", "patrol"),
                        new PedData("mp_f_freemode_01", 5, "lspd_cop", "patrol_stun"),
                    }, new NumPeds(2, 2)),
                    new Loadout("Transport Unit", new List<VehicleData>
                    {
                        new VehicleData("policet")
                    }, new List<PedData>
                    {
                        new PedData("mp_m_freemode_01", 100, "lspd_cop", null),
                    }, new NumPeds(1, 2))
                });
            var highwayAgency = new Agency("San Andreas Highway Patrol", "SAHP", "sahp", "default", "s_m_y_hwaycop_01", "decl_diff_001_a_uni", new List<Loadout>
            {
                new Loadout("Motor Unit", new List<VehicleData>
                {
                    new VehicleData("policeb")
                }, new List<PedData>
                {
                    new PedData("s_m_y_hwaycop_01", 100, null, "biker", true)
                }, new NumPeds(1, 1))
            });
            var data = IoC.Instance.GetInstance<ILspdfrData>();

            var result = data.Agencies;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(lspdAgency, result.Items.First(x => x.ScriptName.Equals("lspd")));
            Xunit.Assert.Equal(highwayAgency, result.Items.First(x => x.ScriptName.Equals("sahp")));
        }

        [Fact]
        public void TestInventoryDeserialization()
        {
            var patrol = new Inventory("Patrol", "patrol", new List<WeaponData>
            {
                new WeaponData("WEAPON_PUMPSHOTGUN", 10),
                new WeaponData("WEAPON_COMBATPISTOL", 45),
                new WeaponData("WEAPON_PISTOL", 45)
            }, new WeaponData("WEAPON_STUNGUN"));
            var biker = new Inventory("Motor Patrol", "biker", new List<WeaponData>
            {
                new WeaponData("WEAPON_COMBATPISTOL")
            });
            var data = IoC.Instance.GetInstance<ILspdfrData>();

            var result = data.Inventories;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(patrol, result.Items.First(x => x.ScriptName.Equals("patrol")));
            Xunit.Assert.Equal(biker, result.Items.First(x => x.ScriptName.Equals("biker")));
        }
    }
}