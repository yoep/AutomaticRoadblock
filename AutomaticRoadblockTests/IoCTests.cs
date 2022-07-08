using AutomaticRoadblock;
using AutomaticRoadblock.AbstractionLayer;
using AutomaticRoadblock.Settings;
using AutomaticRoadblockTests.Model;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace AutomaticRoadblockTests
{
    public class IoCTests
    {
        public class Register
        {
            [Fact]
            public void ShouldReturnDifferentInstances()
            {
                var ioC = IoC.Instance;
                ioC
                    .UnregisterAll()
                    .RegisterInstance<INotification>(Mock.Of<INotification>())
                    .RegisterInstance<ILogger>(Mock.Of<ILogger>())
                    .Register<ISettingsManager>(typeof(SettingsManager));
                var expectedResult = ioC.GetInstance<ISettingsManager>();

                var result = ioC.GetInstance<ISettingsManager>();

                Assert.NotEqual(expectedResult, result);
            }
        }

        public class RegisterSingleton
        {
            [Fact]
            public void ShouldReturnSingletonInstance()
            {
                var ioC = IoC.Instance;
                ioC
                    .UnregisterAll()
                    .RegisterInstance<INotification>(Mock.Of<INotification>())
                    .RegisterInstance<ILogger>(Mock.Of<ILogger>())
                    .RegisterSingleton<ISettingsManager>(typeof(SettingsManager));
                var expectedResult = ioC.GetInstance<ISettingsManager>();

                var result = ioC.GetInstance<ISettingsManager>();

                Assert.Equal(expectedResult, result);
            }
        }

        public class PostConstruct
        {
            [Fact]
            public void ShouldInvokePostConstructMethod()
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
}