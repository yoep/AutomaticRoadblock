using System;
using Rage;

namespace AutomaticRoadblocks.AbstractionLayer.Implementation
{
    public class RageLogger : ILogger
    {
        const string LevelWarn = "WARN";
        const string LevelError = "ERROR";
        
        public void Trace(string message)
        {
            Game.LogTrivialDebug(BuildMessage("TRACE", message));
        }

        public void Debug(string message)
        {
            Game.LogTrivialDebug(BuildMessage("DEBUG", message));
        }

        public void Info(string message)
        {
            Game.LogTrivial(BuildMessage("INFO", message));
        }

        public void Warn(string message)
        {
            Game.LogTrivial(BuildMessage(LevelWarn, message));
        }

        public void Warn(string message, Exception exception)
        {
            Game.LogTrivial(BuildMessage(LevelWarn, message, exception));
        }

        public void Error(string message)
        {
            Game.LogTrivial(BuildMessage(LevelError, message));
        }

        public void Error(string message, Exception exception)
        {
            Game.LogTrivial(BuildMessage(LevelError, message, exception));
        }

        private static string BuildMessage(string level, string message, Exception exception = null)
        {
            var stacktrace = exception?.StackTrace;

            return $"{AutomaticRoadblocksPlugin.Name}: [{level}] {message}\n{stacktrace}";
        }
    }
}