using System;
using System.Collections.Generic;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Vehicles
{
    public static class VehicleFactory
    {
        private static readonly IDictionary<VehicleType, Func<Vector3, Model>> VehicleModels = new Dictionary<VehicleType, Func<Vector3, Model>>
        {
            // a null model is not allowed as Rage will always try to load in a model
            // so we use a placeholder instead
            { VehicleType.None, _ => ModelUtils.Vehicles.GetStatePoliceVehicle() },
            { VehicleType.LocalUnit, ModelUtils.Vehicles.GetLocalPoliceVehicle },
            { VehicleType.PrisonerTransport, _ => ModelUtils.Vehicles.GetTransporterPoliceVehicle() },
            { VehicleType.StateUnit, _ => ModelUtils.Vehicles.GetStatePoliceVehicle() },
            { VehicleType.SwatTeam, _ => ModelUtils.Vehicles.GetFbiPoliceVehicle() },
            { VehicleType.NooseTeam, _ => ModelUtils.Vehicles.GetSwatPoliceVehicle() },
        };

        private static readonly IDictionary<VehicleType, Func<Vector3, float, bool, ARVehicle>> Vehicles =
            new Dictionary<VehicleType, Func<Vector3, float, bool, ARVehicle>>
            {
                // a null model is not allowed as Rage will always try to load in a model
                // so we use a placeholder instead
                { VehicleType.None, (_, _, _) => null },
                {
                    VehicleType.LocalUnit,
                    (position, heading, recordCollisions) =>
                        DoInternalVehicleCreation(CreateModel(VehicleType.LocalUnit, position), position, heading, recordCollisions)
                },
                {
                    VehicleType.PrisonerTransport,
                    (position, heading, recordCollisions) =>
                        DoInternalVehicleCreation(CreateModel(VehicleType.PrisonerTransport, position), position, heading, recordCollisions)
                },
                {
                    VehicleType.StateUnit,
                    (position, heading, recordCollisions) =>
                        DoInternalVehicleCreation(CreateModel(VehicleType.StateUnit, position), position, heading, recordCollisions)
                },
                {
                    VehicleType.SwatTeam,
                    (position, heading, recordCollisions) =>
                        DoInternalVehicleCreation(CreateModel(VehicleType.SwatTeam, position), position, heading, recordCollisions)
                },
                {
                    VehicleType.NooseTeam,
                    (position, heading, recordCollisions) =>
                        DoInternalVehicleCreation(CreateModel(VehicleType.NooseTeam, position), position, heading, recordCollisions)
                },
            };

        /// <summary>
        /// Create the vehicle model for the given vehicle type.
        /// </summary>
        /// <param name="vehicleType">The vehicle type of the model.</param>
        /// <param name="position">The postion for locale models.</param>
        /// <returns>Returns the created model.</returns>
        public static Model CreateModel(VehicleType vehicleType, Vector3 position)
        {
            Assert.NotNull(vehicleType, "vehicleType cannot be null");
            Assert.NotNull(position, "position cannot be null");
            return VehicleModels[vehicleType].Invoke(position);
        }

        /// <summary>
        /// Create a new vehicle instance for the given vehicle type.
        /// </summary>
        /// <param name="vehicleType">The vehicle type to create.</param>
        /// <param name="position">The position of the vehicle.</param>
        /// <param name="heading">The heading of the vehicle.</param>
        /// <param name="recordCollisions">Indicate if collisions should be recorded for this vehicle (memory intensive).</param>
        /// <returns>Returns the created vehicle instance.</returns>
        public static ARVehicle Create(VehicleType vehicleType, Vector3 position, float heading, bool recordCollisions = false)
        {
            Assert.NotNull(vehicleType, "vehicleType cannot be null");
            Assert.NotNull(position, "position cannot be null");
            return Vehicles[vehicleType].Invoke(position, heading, recordCollisions);
        }

        /// <summary>
        /// Create a new vehicle instance for the given vehicle type.
        /// </summary>
        /// <param name="vehicleModel">The vehicle model to use for creating the vehicle.</param>
        /// <param name="position">The position of the vehicle.</param>
        /// <param name="heading">The heading of the vehicle.</param>
        /// <param name="recordCollisions">Indicate if collisions should be recorded for this vehicle (memory intensive).</param>
        /// <returns>Returns the created vehicle instance.</returns>
        public static ARVehicle CreateWithModel(Model vehicleModel, Vector3 position, float heading, bool recordCollisions = false)
        {
            Assert.NotNull(vehicleModel, "vehicleModel cannot be null");
            Assert.NotNull(position, "position cannot be null");
            return DoInternalVehicleCreation(vehicleModel, position, heading, recordCollisions);
        }

        private static ARVehicle DoInternalVehicleCreation(Model vehicleModel, Vector3 position, float heading, bool recordCollisions)
        {
            return new ARVehicle(vehicleModel, GameUtils.GetOnTheGroundPosition(position), heading, recordCollisions);
        }
    }
}