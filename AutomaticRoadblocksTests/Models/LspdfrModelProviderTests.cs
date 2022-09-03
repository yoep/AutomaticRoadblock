using AutomaticRoadblocks;
using AutomaticRoadblocks.Models;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace AutomaticRoadblocksTests.Models
{
    public class LspdfrModelProviderTests
    {
        public LspdfrModelProviderTests(ITestOutputHelper testOutputHelper)
        {
            TestUtils.InitializeIoC();
            TestUtils.SetLogger(testOutputHelper);
        }

        [Fact]
        public void TestAgencyDeserialization()
        {
            var provider = IoC.Instance.GetInstance<IModelProvider>();

            Assert.NotNull(provider.Agencies);
        }
    }
}