using Xunit;
using Xunit.Abstractions;

namespace AutomaticRoadblocks.Barriers
{
    public class BarrierModelDataTests
    {
        public BarrierModelDataTests(ITestOutputHelper testOutputHelper)
        {
            TestUtils.InitializeIoC();
            TestUtils.SetLogger(testOutputHelper);
        }

        [Fact]
        public void TestBarriersDeserialization()
        {
            var expectedResult = new Barrier("Small cone", "small_cone", "prop_mp_cone_03", 0.4, EBarrierFlags.ManualPlacement | EBarrierFlags.RedirectTraffic);
            var provider = (BarrierModelData)IoC.Instance.GetInstance<IBarrierModelData>();

            var result = provider.Barriers;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(expectedResult, result.Items[0]);
        }
    }
}