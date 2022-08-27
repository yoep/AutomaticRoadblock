using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Type;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    internal static class RoadblockHelpers
    {
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        internal static void ReleaseInstancesToLspdfr(List<InstanceSlot> instances, Vehicle vehicle)
        {
            var copPeds = instances
                .Where(x => x.Type == EntityType.CopPed)
                .Select(x => x.Instance)
                .Select(x => (ARPed)x)
                .Select(x => x.GameInstance)
                .ToList();

            // make sure the cops are in a vehicle when releasing them
            Logger.Trace($"Releasing a total of {copPeds.Count} cops to LSPDFR");
            copPeds
                .Select(x => (Ped)x)
                .ToList()
                .ForEach(x =>
                {
                    // make sure the ped is the vehicle or at least entering it
                    if (!x.IsInVehicle(vehicle, true))
                        x.Tasks.EnterVehicle(vehicle, (int)VehicleSeat.Any);
                });

            // release the cops & cop vehicles
            instances
                .Where(x => x.Type is EntityType.CopPed or EntityType.CopVehicle)
                .Select(x => x.Instance)
                .ToList()
                .ForEach(x => x.Release());

            // remove all cop instances so that we don't remove them by accident when disposing
            // these instances are now in control of LSPDFR
            instances.RemoveAll(x => x.Type is EntityType.CopPed or EntityType.CopVehicle);
        }

        /// <summary>
        /// Get a police cop ped model for the current vehicle model.
        /// </summary>
        /// <param name="vehicleModel">The vehicle model to retrieve the cop model for.</param>
        /// <param name="roadblockPosition">The roadblock position to determine locale cop models.</param>
        /// <returns>Returns a cop ped model.</returns>
        internal static Model GetPedModelForVehicle(Model vehicleModel, Vector3 roadblockPosition)
        {
            Assert.NotNull(vehicleModel, "vehicleModel cannot be null");
            Assert.NotNull(roadblockPosition, "roadblockPosition cannot be null");
            var model = vehicleModel.Name;

            if (ModelUtils.IsBike(model))
            {
                return ModelUtils.Peds.GetPoliceBikeCop();
            }

            if (ModelUtils.Vehicles.CityVehicleModels.Contains(model) || ModelUtils.Vehicles.CountyVehicleModels.Contains(model) ||
                ModelUtils.Vehicles.StateVehicleModels.Contains(model))
            {
                return ModelUtils.Peds.GetLocalCop(roadblockPosition);
            }

            return ModelUtils.Vehicles.FbiVehicleModels.Contains(model)
                ? ModelUtils.Peds.GetPoliceFbiCop()
                : ModelUtils.Peds.GetPoliceSwatCop();
        }
    }
}