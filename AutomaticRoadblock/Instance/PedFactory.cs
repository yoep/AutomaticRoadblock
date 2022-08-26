using AutomaticRoadblocks.Utils;

namespace AutomaticRoadblocks.Instance
{
    public static class PedFactory
    {
        /// <summary>
        /// Create weapons for the given ped based on its model.
        /// </summary>
        /// <param name="ped">The ped instance to give weapons.</param>
        /// <returns>Returns the ped instance.</returns>
        public static ARPed CreateCopWeaponsForModel(ARPed ped)
        {
            Assert.NotNull(ped, "ped cannot be null");
            var model = ped.GameInstance.Model;

            return ModelUtils.Peds.IsFbi(model) || ModelUtils.Peds.IsSwat(model)
                ? CreateSpecialCopWeapons(ped)
                : CreateBasicCopWeapons(ped);
        }

        /// <summary>
        /// Create cops weapons for the given ped.
        /// </summary>
        /// <param name="ped">The ped for which cop weapons should be given.</param>
        /// <returns>Returns the ped instance.</returns>
        public static ARPed CreateBasicCopWeapons(ARPed ped)
        {
            ped.GivePrimaryWeapon(ModelUtils.Weapons.Pistol);
            ped.GiveWeapon(ModelUtils.Weapons.Nightstick);
            ped.GiveWeapon(ModelUtils.Weapons.StunGun);
            return ped;
        }

        /// <summary>
        /// Create cops weapons for the given ped.
        /// </summary>
        /// <param name="ped">The ped for which cop weapons should be given.</param>
        /// <returns>Returns the ped instance.</returns>
        public static ARPed CreateSpecialCopWeapons(ARPed ped)
        {
            ped.GivePrimaryWeapon(ModelUtils.Weapons.HeavyRifle);
            ped.GiveWeapon(ModelUtils.Weapons.Shotgun);
            return ped;
        }
    }
}