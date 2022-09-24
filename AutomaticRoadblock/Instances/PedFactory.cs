using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Instances
{
    public static class PedFactory
    {
        /// <summary>
        /// Convert the given ped entity to a ped instance.
        /// </summary>
        /// <param name="ped">The ped entity.</param>
        /// <param name="position">The position of the ped instance.</param>
        /// <returns>Returns the ped instance.</returns>
        public static ARPed ToInstance(Ped ped, Vector3 position, float heading)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Assert.NotNull(position, "position cannot be null");
            return new ARPed(ped, GameUtils.GetOnTheGroundPosition(position), heading);
        }
    }
}