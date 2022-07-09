using AutomaticRoadblocks;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Pursuit;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblockTests.AbstractionLayer.Implementation;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace AutomaticRoadblockTests.Pursuit
{
    public class PursuitManagerTests
    {
        public PursuitManagerTests()
        {
            IoC.Instance
                .Register<ILogger>(typeof(SystemLogger))
                .RegisterInstance<IGame>(Mock.Of<IGame>())
                .RegisterInstance<ISettingsManager>(Mock.Of<ISettingsManager>())
                .RegisterInstance<IRoadblockDispatcher>(Mock.Of<IRoadblockDispatcher>())
                .RegisterSingleton<IPursuitManager>(typeof(PursuitManager));
        }

        [Fact]
        public void TestDispatchNow_WhenNoPursuitIsActive_ShouldReturnFalse()
        {
            var manager = IoC.Instance.GetInstance<IPursuitManager>();

            var result = manager.DispatchNow();
            
            Assert.False(result, "Expected no roadblock to have been dispatched");
        }
    }
}