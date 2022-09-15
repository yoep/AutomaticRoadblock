using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AutomaticRoadblocks.Barriers
{
    public class BarrierDataTests
    {
        public BarrierDataTests(ITestOutputHelper testOutputHelper)
        {
            TestUtils.InitializeIoC();
            TestUtils.SetLogger(testOutputHelper);
        }

        [Fact]
        public void TestBarriersDeserialization()
        {
            var expectedResult = new Barrier("Small cone", "small_cone", "prop_mp_cone_03", 0.4, EBarrierFlags.ManualPlacement | EBarrierFlags.RedirectTraffic);
            var modelData = IoC.Instance.GetInstance<IBarrierData>();

            var result = modelData.Barriers;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.NotEqual(Barriers.Defaults, result);
            Xunit.Assert.Equal(expectedResult, result.Items[0]);
        }

        [Fact]
        public void TestBarrierFlagsDeserialization()
        {
            var modelData = IoC.Instance.GetInstance<IBarrierData>();

            var result = modelData.Barriers;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(EBarrierFlags.All, result.Items.First(x => x.ScriptName.Equals("small_cone")).Flags);
            Xunit.Assert.Equal(EBarrierFlags.ManualPlacement, result.Items.First(x => x.ScriptName.Equals("police_do_not_cross")).Flags);
        }
    }
}