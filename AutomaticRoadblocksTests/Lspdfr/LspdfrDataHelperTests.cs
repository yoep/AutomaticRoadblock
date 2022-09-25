using System;
using System.Linq;
using LSPD_First_Response.Engine.Scripting;
using Xunit;
using Xunit.Abstractions;

namespace AutomaticRoadblocks.Lspdfr
{
    public class LspdfrDataHelperTests
    {
        public LspdfrDataHelperTests(ITestOutputHelper testOutputHelper)
        {
            TestUtils.InitializeIoC();
            TestUtils.SetLogger(testOutputHelper);
        }

        /// <summary>
        /// <see cref="LspdfrDataHelper.RetrieveLoadout"/>
        /// </summary>
        [Fact]
        public void TestLoadoutTransporterRetrieval()
        {
            var lspdfrData = IoC.Instance.GetInstance<ILspdfrData>();
            var backup = lspdfrData.BackupUnits.LocalPatrol;
            var agency = lspdfrData.Agencies[backup[EWorldZoneCounty.LosSantos]];

            var result = agency.Loadout.FirstOrDefault(x => x.Name.Equals(Agency.TransporterLoadout, StringComparison.InvariantCulture));

            Xunit.Assert.NotNull(result);
        }
    }
}