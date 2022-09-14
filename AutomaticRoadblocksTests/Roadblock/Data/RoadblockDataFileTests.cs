using System.Linq;
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
            var expectedResult = new RoadblockData(4, "police_do_not_cross", "barrel_traffic_catcher");
            var data = IoC.Instance.GetInstance<IRoadblockData>();

            var result = data.Roadblocks;
            
            Xunit.Assert.NotNull(result);
            Xunit.Assert.NotEqual(Roadblocks.Defaults, result);
            Xunit.Assert.Equal(expectedResult, result.Items.First(x => x.Level == expectedResult.Level));
        }
    }
}