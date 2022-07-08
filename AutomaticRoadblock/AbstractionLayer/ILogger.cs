using System;

namespace AutomaticRoadblocks.AbstractionLayer
{
    public interface ILogger
    {
        void Trace(string message);
        
        void Debug(string message);
        
        void Info(string message);

        void Warn(string message);
        
        void Warn(string message, Exception exception);
        
        void Error(string message);
        
        void Error(string message, Exception exception);
    }
}