using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Logging;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Vehicles;

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
                // always cancel all tasks of the cops
                Logger.Trace($"Clearing all tasks for a total of {copInstances.Count} cops");
                foreach (var ped in copInstances)
                {
                    ped.ClearAllTasks(true);
                }

                // command the cops to start entering their vehicle if possible
                // if not, due to no valid vehicle, release all cops back to the world
                if (vehicle is { IsInvalid: false })
                {
                    for (var i = 0; i < copInstances.Where(x => x is { IsInvalid: false }).ToList().Count; i++)
                    {
                        var cop = copInstances[i];
                        TryEnteringVehicle(cop, vehicle, i == 0 ? EVehicleSeat.Driver : EVehicleSeat.Any);
                    }
                }
                else
                {
                    // release the cop instances
                    Logger.Trace($"Releasing a total of {copInstances.Count} cops to LSPDFR");
                    copInstances
                        .Where(x => x is { IsInvalid: false })
                        .ToList()
                        .ForEach(x => x.Release());
                }

                Logger.Info("Instances have been released to LSPDFR");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to release instances, {ex.Message}", ex);
                GameUtils.DisplayNotificationDebug("~r~Failed to release instances to LSPDFR");
            }
        }

        private static void TryEnteringVehicle(ARPed ped, ARVehicle vehicle, EVehicleSeat vehicleSeat)
        {
            GameUtils.NewSafeFiber(() =>
            {
                if (vehicle is { IsInvalid: false } && !ped.IsInvalid)
                {
                    var instance = ped.GameInstance;
                    var vehicleInstance = vehicle.GameInstance;

                    // make the cop enter the vehicle if it isn't already
                    // before doing this, all running tasks should have been cancelled upfront
                    if (!instance.IsInVehicle(vehicleInstance, true))
                        instance.Tasks.EnterVehicle(vehicleInstance, 15000, (int)vehicleSeat).WaitForCompletion();

                    // release the vehicle from this plugin once the driver has entered
                    // this should prevent the vehicle from despawning too fast before the cops were able to enter it
                    if (vehicleSeat == EVehicleSeat.Driver)
                        vehicle.Release();

                    // release the ped from this plugin
                    ped.Release();
                }
                else
                {
                    Logger.Error("Unable to release cop instances correctly, cannot enter vehicle as it's null or invalid");
                }
            }, "RoadblockHelpers.TryEnteringVehicle");
        }
    }
}