using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Logging;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Vehicles;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    internal static class RoadblockHelpers
    {
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        /// <summary>
        /// Release the given instances back to LSPDFR.
        /// </summary>
        /// <param name="copInstances">The instances which are allowed to be released.</param>
        /// <param name="vehicle">The vehicle instance which will be released, can be null.</param>
        /// <returns>Returns the instances which have been released.</returns>
        internal static void ReleaseInstancesToLspdfr(IList<ARPed> copInstances, ARVehicle vehicle)
        {
            Assert.NotNull(copInstances, "copInstances cannot be null");
            try
            {
                // release the cops instances
                copInstances
                    .Where(x => x is { IsInvalid: false })
                    .ToList()
                    .ForEach(x => x.Release());

                // release the vehicle instance
                if (vehicle != null)
                    vehicle.Release();

                // make sure the cops are in a vehicle when releasing them
                Logger.Trace($"Releasing a total of {copInstances.Count} cops to LSPDFR");

                if (vehicle != null)
                {
                    for (var i = 0; i < copInstances.Where(x => x is { IsInvalid: false }).ToList().Count; i++)
                    {
                        var cop = copInstances[i];
                        TryEnteringVehicle(cop, vehicle.GameInstance, i == 0 ? EVehicleSeat.Driver : EVehicleSeat.Any);
                    }
                }

                Logger.Info("Instances have been released to LSPDFR");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to release instances, {ex.Message}", ex);
                GameUtils.DisplayNotificationDebug("~r~Failed to release instances to LSPDFR");
            }
        }

        private static void TryEnteringVehicle(ARPed ped, Vehicle vehicle, EVehicleSeat vehicleSeat)
        {
            GameUtils.NewSafeFiber(() =>
            {
                if (vehicle != null && vehicle.IsValid() && !ped.IsInvalid)
                {
                    var instance = ped.GameInstance;

                    if (!instance.IsInVehicle(vehicle, true))
                    {
                        instance.Tasks.EnterVehicle(vehicle, 20000, (int)vehicleSeat).WaitForCompletion();
                    }

                    if (vehicleSeat == EVehicleSeat.Driver)
                        Functions.SetCopAsBusy(instance, false);
                }
                else
                {
                    Logger.Error("Unable to release cop instances correctly, cannot enter vehicle as it's null or invalid");
                }
            }, "RoadblockHelpers.TryEnteringVehicle");
        }
    }
}