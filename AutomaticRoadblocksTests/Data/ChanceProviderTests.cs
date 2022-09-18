using System.Collections.Generic;
using AutomaticRoadblocks.Roadblock.Data;
using Xunit;
using Xunit.Abstractions;

namespace AutomaticRoadblocks.Data
{
    public class ChanceProviderTests
    {
        public ChanceProviderTests(ITestOutputHelper testOutputHelper)
        {
            TestUtils.InitializeIoC();
            TestUtils.SetLogger(testOutputHelper);
        }

        [Fact]
        public void TestRetrieveItemFromValidList()
        {
            var items = new List<Unit>
            {
                new Unit("A", 50),
                new Unit("B", 40),
                new Unit("C", 5),
                new Unit("D", 5),
            };

            var result = ChanceProvider.Retrieve(items);

            Xunit.Assert.NotNull(result);
        }

        [Fact]
        public void TestRetrieveWhenItemHas100PercentChanceShouldReturnTheItem()
        {
            var expectedItem = new Unit("A", 100);
            var items = new List<Unit>
            {
                expectedItem
            };

            var result = ChanceProvider.Retrieve(items);

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(expectedItem, result);
        }

        [Fact]
        public void TestRetrieveWhenListIsInvalidShouldReturnChanceException()
        {
            var items = new List<Unit>
            {
                new Unit("A", 0)
            };

            Xunit.Assert.Throws<ChanceException<Unit>>(() => ChanceProvider.Retrieve(items));
        }
    }
}