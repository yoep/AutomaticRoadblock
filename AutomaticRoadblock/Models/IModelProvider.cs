using Rage;

namespace AutomaticRoadblocks.Models
{
    /// <summary>
    /// The model provider convert the <see cref="IModelData"/> into an actual usable <see cref="Model"/>.
    /// </summary>
    public interface IModelProvider
    {
        Model LocalCopPed(Vector3 position, EUnitType type);

        Model LocalCopVehicle(Vector3 position, EUnitType type);
    }
}