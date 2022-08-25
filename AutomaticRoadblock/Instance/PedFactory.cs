using AutomaticRoadblocks.Utils;

namespace AutomaticRoadblocks.Instance
{
    public static class PedFactory
    {
        /// <summary>
        /// Create cops weapons for the given ped.
        /// </summary>
        /// <param name="ped">The ped for which cop weapons should be given.</param>
        /// <returns>Returns the ped instance.</returns>
        public static ARPed CreateCopWeapons(ARPed ped)
        {
            ped.GivePrimaryWeapon(ModelUtils.Weapons.Pistol);
            ped.GiveWeapon(ModelUtils.Weapons.Nightstick);
            ped.GiveWeapon(ModelUtils.Weapons.StunGun);
            return ped;
        }
    }
}