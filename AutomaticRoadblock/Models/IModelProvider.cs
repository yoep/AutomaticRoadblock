using Rage;

namespace AutomaticRoadblocks.Models
{
    /// <summary>
    /// The model provider convert the <see cref="IModelData"/> into an actual usable <see cref="Model"/>.
    /// </summary>
    public interface IModelProvider
    {
        /// <summary>
        /// Retrieve a ped model for the given postion and type.
        /// </summary>
        /// <param name="position">The position of the model.</param>
        /// <param name="type">The type of the model.</param>
        /// <returns>Returns the cop ped model.</returns>
        /// <exception cref="NoModelAvailableException">Is thrown when no model if configured for the given criteria.</exception>
        Model CopPed(Vector3 position, EUnitType type);

        /// <summary>
        /// Retrieve a vehicle model for the given postion and type.
        /// </summary>
        /// <param name="position">The position of the model.</param>
        /// <param name="type">The type of the model.</param>
        /// <returns>Returns the cop vehicle model.</returns>
        /// <exception cref="NoModelAvailableException">Is thrown when no model if configured for the given criteria.</exception>
        Model CopVehicle(Vector3 position, EUnitType type);
    }
}