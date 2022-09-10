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
            Xunit.Assert.Equal(16, provider.Agencies.Items.Count);
            Xunit.Assert.Equal(60, provider.Agencies.Items[0].Loadout.Peds[0].Chance);
        }

        [Fact]
        public void TestOutfitsDeserialization()
        {
            var provider = (LspdfrModelData)IoC.Instance.GetInstance<IModelData>();

            Xunit.Assert.NotNull(provider.Outfits);
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

            var result = provider.Ped(EUnitType.LocalPatrol, EWorldZoneCounty.LosSantos);
            
            // Xunit.Assert.NotNull(result);
        }
    }
}