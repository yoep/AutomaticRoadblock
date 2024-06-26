using System;

namespace AutomaticRoadblocks.Logging
{
    public interface ILogger
    {
        /// <summary>
        /// The log level of the logger.
        /// </summary>
        ELogLevel LogLevel { get; set; }
        
        void Trace(string message);
        
        void Debug(string message);
        
        void Info(string message);

        void Warn(string message);
        
        void Warn(string message, Exception exception);
        
        void Error(string message);
        
        void Error(string message, Exception exception);
    }
}