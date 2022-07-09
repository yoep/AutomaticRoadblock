using System;
using AutomaticRoadblocks.AbstractionLayer;

namespace AutomaticRoadblockTests.AbstractionLayer.Implementation
{
    public class SystemLogger : ILogger
    {
        public void Trace(string message)
        {
            Console.WriteLine($"[TRACE] {message}");
        }

        public void Debug(string message)
        {
            Console.WriteLine($"[DEBUG] {message}");
        }

        public void Info(string message)
        {
            Console.WriteLine($"[INFO] {message}");
        }

        public void Warn(string message)
        {
            Console.WriteLine($"[WARN] {message}");
        }

        public void Warn(string message, Exception exception)
        {
            Console.WriteLine($"[WARN] {message}", exception);
        }

        public void Error(string message)
        {
            Console.WriteLine($"[ERROR] {message}");
        }

        public void Error(string message, Exception exception)
        {
            Console.WriteLine($"[ERROR] {message}", exception);
        }
    }
}