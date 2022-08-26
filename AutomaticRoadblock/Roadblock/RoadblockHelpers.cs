using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils.Type;
using LSPD_First_Response.Mod.API;
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
                .ToList();

            // release the instances before giving them to LSPDFR
            // this should prevent accidental override of attributes set by LSPDFR
            instances
                .Where(x => x.Type is EntityType.CopPed or EntityType.CopVehicle)
                .Select(x => x.Instance)
                .ToList()
                .ForEach(x => x.Release());

            Logger.Trace($"Releasing a total of {copPeds.Count} to LSPDFR");
            copPeds
                .Select(x => x.Instance)
                .Select(x => x.GameInstance)
                .Select(x => (Ped)x)
                .ToList()
                .ForEach(x =>
                {
                    // make sure the ped is the vehicle or at least entering it
                    if (!x.IsInVehicle(vehicle, true))
                        x.Tasks.EnterVehicle(vehicle, (int)VehicleSeat.Any);

                    Functions.SetPedAsCop(x);
                    Functions.SetCopAsBusy(x, false);
                });

            // remove all cop instances so that we don't remove them by accident
            // these instances are now in control of LSPDFR
            instances.RemoveAll(x => x.Type is EntityType.CopPed or EntityType.CopVehicle);
        }
    }
}