using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Vehicles;
using JetBrains.Annotations;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    internal static class RoadblockHelpers
    {
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        private static readonly IGame Game = IoC.Instance.GetInstance<IGame>();

        /// <summary>
        /// Release the given instances back to LSPDFR.
        /// </summary>
        /// <param name="copInstances">The instances which are allowed to be released.</param>
        /// <param name="vehicle">The vehicle instance which will be released, can be null.</param>
        /// <returns>Returns the instances which have been released.</returns>
        internal static void ReleaseInstancesToLspdfr(IList<ARPed> copInstances, [CanBeNull] ARVehicle vehicle)
        {
            Assert.NotNull(copInstances, "copInstances cannot be null");
            try
            {
                // release the cops & cop vehicle instances
                copInstances
                    .Where(x => x is { IsInvalid: false })
                    .ToList()
                    .ForEach(x => x.Release());

                // make sure the cops are in a vehicle when releasing them
                Logger.Trace($"Releasing a total of {copInstances.Count} cops to LSPDFR");
                copInstances
                    .Where(x => x is { IsInvalid: false })
                    .Select(x => x.GameInstance)
                    .ToList()
                    .ForEach(x =>
                    {
                        // make sure the ped is the vehicle or at least entering it
                        if (vehicle != null)
                            TryEnteringVehicle(x, vehicle.GameInstance);
                    });
                Logger.Info("Instances have been released to LSPDFR");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to release instances, {ex.Message}", ex);
                Game.DisplayNotificationDebug("~r~Failed to release instances to LSPDFR");
            }
        }

        private static void TryEnteringVehicle(Ped ped, Vehicle vehicle)
        {
            if (vehicle != null && vehicle.IsValid())
            {
                if (!ped.IsInVehicle(vehicle, true))
                {
                    ped.Tasks.EnterVehicle(vehicle, 3000, (int)EVehicleSeat.Any);
                }
            }
            else
            {
                Logger.Error("Unable to release cop instances correctly, cannot enter vehicle as it's null or invalid");
            }
        }
    }
}