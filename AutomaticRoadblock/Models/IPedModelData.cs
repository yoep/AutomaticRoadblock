using LSPD_First_Response.Engine.Scripting;

namespace AutomaticRoadblocks.Models
{
    public interface IPedModelData : IModelData
    {
        /// <summary>
        /// Retrieve the ped model info for the given filters.
        /// </summary>
        /// <param name="unitType">The unit type to retrieve.</param>
        /// <param name="zone">The zone for which the model should be retrieved.</param>
        /// <exception cref="NoModelAvailableException">Is thrown when no model data could be found for the given filters.</exception>
        PedModelInfo Ped(EUnitType unitType, EWorldZoneCounty zone);
    }
}