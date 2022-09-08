using AutomaticRoadblocks;
using AutomaticRoadblocks.Models;
using AutomaticRoadblocks.Models.Lspdfr;
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
        public void TestAgenciesDeserialization()
        {
            var provider = (LspdfrModelProvider)IoC.Instance.GetInstance<IModelProvider>();

            Assert.NotNull(provider.Agencies);
            Assert.Equal(16, provider.Agencies.Items.Count);
        }

        [Fact]
        public void TestOutfitsDeserialization()
        {
            var provider = (LspdfrModelProvider)IoC.Instance.GetInstance<IModelProvider>();

            Assert.NotNull(provider.Outfits);
        }
    }
}