using System;

namespace AutomaticRoadblocks.Roadblock
{
    public class RoadblockException : Exception
    {
        public RoadblockException(string message) : base(message)
        {
        }

        public RoadblockException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}