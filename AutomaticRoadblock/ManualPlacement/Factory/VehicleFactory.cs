using System;
using System.Collections.Generic;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.ManualPlacement.Factory
{
    public static class VehicleFactory
    {
        private static IDictionary<VehicleType, Func<Vector3, Model>> Vehicles = new Dictionary<VehicleType, Func<Vector3, Model>>
        {
            { VehicleType.None, _ => null },
            { VehicleType.Locale, position => ModelUtils.Vehicles.GetLocalPoliceVehicle(position) },
            { VehicleType.State, _ => ModelUtils.Vehicles.GetStatePoliceVehicle() },
            { VehicleType.Fbi, _ => ModelUtils.Vehicles.GetFbiPoliceVehicle() },
            { VehicleType.Swat, _ => ModelUtils.Vehicles.GetSwatPoliceVehicle() },
        };

        public static Model Create(VehicleType vehicleType, Vector3 position)
        {
            Assert.NotNull(vehicleType, "vehicleType cannot be null");
            return Vehicles[vehicleType].Invoke(position);
        }
    }
}