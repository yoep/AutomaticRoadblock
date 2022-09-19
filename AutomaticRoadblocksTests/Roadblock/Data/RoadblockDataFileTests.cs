using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using Xunit;
using Xunit.Abstractions;

namespace AutomaticRoadblocks.Roadblock.Data
{
    public class RoadblockDataFileTests
    {
        public RoadblockDataFileTests(ITestOutputHelper testOutputHelper)
        {
            TestUtils.InitializeIoC();
            TestUtils.SetLogger(testOutputHelper);
        }

        [Fact]
        public void TestRoadblockDeserialization()
        {
            var expectedLevel1Result = new RoadblockData(1, "small_cone_stripes",
                new List<string>
                {
                    Light.FlaresScriptName
                },
                new List<Unit>
                {
                    new Unit(EBackupUnit.LocalPatrol)
                });
            var expectedLevel5Result = new RoadblockData(5, "police_do_not_cross", "work_barrier_high", "barrel_traffic_catcher",
                new List<string>
                {
                    Light.SpotsScriptName
                },
                new List<Unit>
                {
                    new Unit(EBackupUnit.NooseSWAT, 70),
                    new Unit(EBackupUnit.LocalSWAT, 30),
                });
            var data = IoC.Instance.GetInstance<IRoadblockData>();

            var result = data.Roadblocks;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.NotEqual(Roadblocks.Defaults, result);
            Xunit.Assert.Equal(expectedLevel1Result, result.Items.First(x => x.Level == expectedLevel1Result.Level));
            Xunit.Assert.Equal(expectedLevel5Result, result.Items.First(x => x.Level == expectedLevel5Result.Level));
        }
    }
}