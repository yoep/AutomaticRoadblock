using System;

namespace AutomaticRoadblocks.Pursuit
{
    public class NoPursuitActiveException : Exception
    {
        public NoPursuitActiveException() : base("There is currently no active ongoing pursuit")
        {
        }
    }
}