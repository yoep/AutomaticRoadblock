using System;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Roadblock.Data;
using Xunit.Abstractions;

namespace AutomaticRoadblocks
{
    public static class TestUtils
    {
        public static void InitializeIoC()
        {
            IoC.Instance
                .UnregisterAll()
                .RegisterSingleton<ILogger>(typeof(ConsoleLogger))
                .RegisterSingleton<IBarrierData>(typeof(BarrierDataFile))
                .RegisterSingleton<IRoadblockData>(typeof(RoadblockDataFile));
        }

        public static void SetLogger(ITestOutputHelper testOutputHelper)
        {
            var logger = (ConsoleLogger)IoC.Instance.GetInstance<ILogger>();
            logger.SetTestLogger(testOutputHelper);
        }
    }

    public class ConsoleLogger : ILogger
    {
        private ITestOutputHelper _testOutputHelper;

        public void SetTestLogger(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        // ignore
        public ELogLevel LogLevel { get; set; }

        public void Trace(string message)
        {
            _testOutputHelper?.WriteLine("[TRACE] " + message);
            Console.WriteLine("[TRACE] " + message);
        }

        public void Debug(string message)
        {
            _testOutputHelper?.WriteLine("[DEBUG] " + message);
            Console.WriteLine("[DEBUG] " + message);
        }

        public void Info(string message)
        {
            _testOutputHelper?.WriteLine("[INFO] " + message);
            Console.WriteLine("[INFO] " + message);
        }

        public void Warn(string message)
        {
            _testOutputHelper?.WriteLine("[WARN] " + message);
            Console.WriteLine("[WARN] " + message);
        }

        public void Warn(string message, Exception exception)
        {
            _testOutputHelper?.WriteLine("[WARN] " + message + Environment.NewLine + exception.StackTrace);
            Console.WriteLine("[WARN] " + message + Environment.NewLine + exception.StackTrace);
        }

        public void Error(string message)
        {
            _testOutputHelper?.WriteLine("[ERROR] " + message);
            Console.WriteLine("[ERROR] " + message);
        }

        public void Error(string message, Exception exception)
        {
            _testOutputHelper?.WriteLine("[ERROR] " + message + Environment.NewLine + exception.StackTrace);
            Console.WriteLine("[ERROR] " + message + Environment.NewLine + exception.StackTrace);
        }
    }
}