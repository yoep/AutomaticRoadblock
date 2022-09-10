using System;

namespace AutomaticRoadblocks.Models
{
    public class ModelInfoNotFoundException : Exception
    {
        public ModelInfoNotFoundException(string message) 
            : base(message)
        {
        }
    }
}