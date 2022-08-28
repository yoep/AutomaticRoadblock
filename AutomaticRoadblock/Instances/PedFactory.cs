using System.Linq;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Instances
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

        /// <summary>
        /// Get a police cop ped model for the current vehicle model.
        /// </summary>
        /// <param name="vehicleModel">The vehicle model to retrieve the cop model for.</param>
        /// <param name="position">The position to determine locale cop models.</param>
        /// <param name="heading">The heading of the ped.</param>
        /// <returns>Returns a cop ped.</returns>
        public static ARPed CreateCopForVehicle(Model vehicleModel, Vector3 position, float heading)
        {
            Assert.NotNull(vehicleModel, "vehicleModel cannot be null");
            Assert.NotNull(position, "roadblockPosition cannot be null");
            var modelName = vehicleModel.Name;
            Model model;

            if (ModelUtils.IsBike(modelName))
            {
                model = ModelUtils.Peds.GetPoliceBikeCop();
            }
            else if (ModelUtils.Vehicles.CityVehicleModels.Contains(modelName) || ModelUtils.Vehicles.CountyVehicleModels.Contains(modelName) ||
                     ModelUtils.Vehicles.StateVehicleModels.Contains(modelName))
            {
                model = ModelUtils.Peds.GetLocalCop(position);
            }
            else
            {
                model = ModelUtils.Vehicles.FbiVehicleModels.Contains(modelName)
                    ? ModelUtils.Peds.GetPoliceFbiCop()
                    : ModelUtils.Peds.GetPoliceSwatCop();
            }

            return new ARPed(model, position, heading);
        }
    }
}