using System;
using Rage;

namespace AutomaticRoadblocks.Lspdfr
{
    public class LspdfrDataException : Exception
    {
        public LspdfrDataException(EBackupUnit unit, Vector3 position)
        : base($"Invalid LSPDFR data found for unit {unit} at position {position}")
        {
            Unit = unit;
            Position = position;
        }
        
        public EBackupUnit Unit { get; }
        
        public Vector3 Position { get; }
    }
}