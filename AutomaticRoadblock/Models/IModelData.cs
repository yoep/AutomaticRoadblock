using LSPD_First_Response.Engine.Scripting;

namespace AutomaticRoadblocks.Models
{
    /// <summary>
    /// The model data which describes how a certain model should be constructed.
    /// The <see cref="PedModelInfo"/> is raw data parsed from data files (currently the LSPDFR data files).
    /// </summary>
    public interface IModelData
    {
        /// <summary>
        /// Retrieve the ped model info for the given filters.
        /// </summary>
        /// <param name="unitType">The unit type to retrieve.</param>
        /// <param name="zone">The zone for which the model should be retrieved.</param>
        /// <exception cref="ModelInfoNotFoundException">Is thrown when no model data could be found for the given filters.</exception>
        PedModelInfo Ped(EUnitType unitType, EWorldZoneCounty zone);

        /// <summary>
        /// Retrieve the vehicle model info for the given filters.
        /// </summary>
        /// <param name="unitType">The unit type to retrieve.</param>
        /// <param name="zone">The zone for which the model should be retrieved.</param>
        /// <exception cref="ModelInfoNotFoundException">Is thrown when no model data could be found for the given filters.</exception>
        VehicleModelInfo Vehicle(EUnitType unitType, EWorldZoneCounty zone);

        /// <summary>
        /// Reload the model data information.
        /// </summary>
        void Reload();
    }
}