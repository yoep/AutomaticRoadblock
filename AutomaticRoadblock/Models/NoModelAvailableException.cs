using LSPD_First_Response.Engine.Scripting;

namespace AutomaticRoadblocks.Models
{
    public class NoModelAvailableException : ModelException
    {
        public NoModelAvailableException(EUnitType unitType, EWorldZoneCounty zone) 
            : base($"No model available for criteria type: {unitType}, zone: {zone}")
        {
            UnitType = unitType;
            Zone = zone;
        }

        public EUnitType UnitType { get; }
        
        private EWorldZoneCounty Zone { get; }
    }
}