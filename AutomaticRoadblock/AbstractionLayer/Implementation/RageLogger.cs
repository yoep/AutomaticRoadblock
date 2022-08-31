using System;
using Rage;

namespace AutomaticRoadblocks.AbstractionLayer.Implementation
{
    public class RageLogger : ILogger
    {
        private const string LevelWarn = "WARN";
        private const string LevelError = "ERROR";

        /// <inheritdoc />
        public LogLevel LogLevel { get; set; } = LogLevel.Trace;

        /// <inheritdoc />
        public void Trace(string message)
        {
            if (LogLevel.HasFlag(LogLevel.Trace))
                Game.LogTrivial(BuildMessage("TRACE", message));
        }

        /// <inheritdoc />
        public void Debug(string message)
        {
            if (LogLevel.HasFlag(LogLevel.Debug))
                Game.LogTrivial(BuildMessage("DEBUG", message));
        }

        /// <inheritdoc />
        public void Info(string message)
        {
            if (LogLevel.HasFlag(LogLevel.Info))
                Game.LogTrivial(BuildMessage("INFO", message));
        }

        /// <inheritdoc />
        public void Warn(string message)
        {
            if (LogLevel.HasFlag(LogLevel.Warn))
                Game.LogTrivial(BuildMessage(LevelWarn, message));
        }

        /// <inheritdoc />
        public void Warn(string message, Exception exception)
        {
            if (LogLevel.HasFlag(LogLevel.Warn))
                Game.LogTrivial(BuildMessage(LevelWarn, message, exception));
        }

        /// <inheritdoc />
        public void Error(string message)
        {
            if (LogLevel.HasFlag(LogLevel.Error))
                Game.LogTrivial(BuildMessage(LevelError, message));
        }

        /// <inheritdoc />
        public void Error(string message, Exception exception)
        {
            if (LogLevel.HasFlag(LogLevel.Error))
                Game.LogTrivial(BuildMessage(LevelError, message, exception));
        }

        private static string BuildMessage(string level, string message, Exception exception = null)
        {
            var stacktrace = exception?.StackTrace;
            var newline = string.IsNullOrWhiteSpace(stacktrace) ? "" : "\n";

            return $"{AutomaticRoadblocksPlugin.Name}: [{level}] {message}{newline}{stacktrace}";
        }
    }
}