using System;
using AutomaticRoadblocks;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblockTests.Model;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace AutomaticRoadblockTests
{
    public class IoCTests : IDisposable
    {
        public void Dispose()
        {
            IoC.Instance.UnregisterAll();
        }

        [Fact]
        public void TestGetInstance_WhenTypeIsNotASingleton_ShouldReturnANewInstance()
        {
            var ioC = IoC.Instance;
            ioC
                .UnregisterAll()
                .RegisterInstance<IGame>(Mock.Of<IGame>())
                .RegisterInstance<ILogger>(Mock.Of<ILogger>())
                .Register<ISettingsManager>(typeof(SettingsManager));
            var expectedResult = ioC.GetInstance<ISettingsManager>();

            var result = ioC.GetInstance<ISettingsManager>();

            Assert.NotEqual(expectedResult, result);
        }

        [Fact]
        public void TestGetInstance_WhenTypeIsSingleton_ShouldReturnTheSameInstance()
        {
            var ioC = IoC.Instance;
            ioC
                .UnregisterAll()
                .RegisterInstance<ILogger>(Mock.Of<ILogger>())
                .RegisterSingleton<ISettingsManager>(typeof(SettingsManager));
            var expectedResult = ioC.GetInstance<ISettingsManager>();

            var result = ioC.GetInstance<ISettingsManager>();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void TestPostConstruct_WhenPostConstructAnnotationIsPresent_ShouldInvokePostConstructMethod()
        {
            var ioC = IoC.Instance;
            ioC
                .UnregisterAll()
                .RegisterSingleton<IPostConstructModel>(typeof(PostConstructModel));

            var result = ioC.GetInstance<IPostConstructModel>();

            Assert.NotNull(result);
            Assert.True(result.IsInitialized);
        }
    }
}