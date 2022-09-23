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
                    }),
                    new Loadout("Transport Unit", new List<VehicleData>
                    {
                        new VehicleData("policet")
                    })
                });
            var highwayAgency = new Agency("San Andreas Highway Patrol", "SAHP", "sahp", "default", "s_m_y_hwaycop_01", "decl_diff_001_a_uni", new List<Loadout>
            {
                new Loadout("Motor Unit", new List<VehicleData>
                {
                    new VehicleData("policeb")
                })
            });
            var data = IoC.Instance.GetInstance<ILspdfrData>();

            var result = data.Agencies;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(lspdAgency, result.Items.First(x => x.ScriptName.Equals("lspd")));
            Xunit.Assert.Equal(highwayAgency, result.Items.First(x => x.ScriptName.Equals("sahp")));
        }
    }
}