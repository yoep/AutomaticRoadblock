using System;
using Rage;

namespace AutomaticRoadblocks.AbstractionLayer.Implementation
{
    public class RageLogger : ILogger
    {
        private const string LevelWarn = "WARN";
        private const string LevelError = "ERROR";

        /// <inheritdoc />
        public ELogLevel LogLevel { get; set; } = ELogLevel.Trace;

        /// <inheritdoc />
        public void Trace(string message)
        {
            if (LogLevel.HasFlag(ELogLevel.Trace))
                Game.LogTrivial(BuildMessage("TRACE", message));
        }

        /// <inheritdoc />
        public void Debug(string message)
        {
            if (LogLevel.HasFlag(ELogLevel.Debug))
                Game.LogTrivial(BuildMessage("DEBUG", message));
        }

        /// <inheritdoc />
        public void Info(string message)
        {
            if (LogLevel.HasFlag(ELogLevel.Info))
                Game.LogTrivial(BuildMessage("INFO", message));
        }

        /// <inheritdoc />
        public void Warn(string message)
        {
            if (LogLevel.HasFlag(ELogLevel.Warn))
                Game.LogTrivial(BuildMessage(LevelWarn, message));
        }

        /// <inheritdoc />
        public void Warn(string message, Exception exception)
        {
            if (LogLevel.HasFlag(ELogLevel.Warn))
                Game.LogTrivial(BuildMessage(LevelWarn, message, exception));
        }

        /// <inheritdoc />
        public void Error(string message)
        {
            if (LogLevel.HasFlag(ELogLevel.Error))
                Game.LogTrivial(BuildMessage(LevelError, message));
        }

        /// <inheritdoc />
        public void Error(string message, Exception exception)
        {
            if (LogLevel.HasFlag(ELogLevel.Error))
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