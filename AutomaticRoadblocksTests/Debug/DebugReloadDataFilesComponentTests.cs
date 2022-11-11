using System.Collections.Generic;
using AutomaticRoadblocks.Data;
using Xunit;
using Xunit.Abstractions;

namespace AutomaticRoadblocks.Debug
{
    public class DebugReloadDataFilesComponentTests
    {
        public DebugReloadDataFilesComponentTests(ITestOutputHelper testOutputHelper)
        {
            TestUtils.InitializeIoC();
            TestUtils.SetLogger(testOutputHelper);
        }

        [Fact]
        public void TestInitializeInstanceWithListArgument()
        {
            var ioC = IoC.Instance;
            ioC.Register<ITest>(typeof(TestClass));
            var instance = ioC.GetInstance<ITest>();

            var result = instance.DataFiles;

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(3, result.Count);
        }

        [Fact]
        public void TestReloadDataFiles()
        {
            var dataFiles = IoC.Instance.GetInstances<IDataFile>();

            foreach (var data in dataFiles)
            {
                data.Reload();
            }
        }

        public interface ITest
        {
            List<IDataFile> DataFiles { get; }
        }

        public class TestClass : ITest
        {
            public TestClass(List<IDataFile> dataFiles)
            {
                DataFiles = dataFiles;
            }

            public List<IDataFile> DataFiles { get; }
        }
    }
}