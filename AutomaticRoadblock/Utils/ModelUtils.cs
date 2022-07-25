using System;
using System.Collections.Generic;
using System.Linq;
using LSPD_First_Response.Engine.Scripting;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AutomaticRoadblocks.Utils
{
    public static class ModelUtils
    {
        private const string PoliceBikeModelName = "POLICEB";
        private const string PoliceTransporterModelName = "POLICET";
        private const string PoliceBikeCopModelName = "s_m_y_hwaycop_01";

        private static readonly Random Random = new();

        public static class Weapons
        {
            public const string Pistol = "weapon_pistol";
            public const string StunGun = "weapon_stungun_mp";
            public const string Shotgun = "weapon_pumpshotgun";
            public const string HeavyRifle = "weapon_heavyrifle";
            public const string Nightstick = "weapon_nightstick";
            public const string MicroSmg = "weapon_microsmg";
            public const string Smg = "weapon_smg";

            public static class Attachments
            {
                public const string PistolFlashlight = "COMPONENT_AT_PI_FLSH";
                public const string RifleFlashlight = "COMPONENT_AT_AR_FLSH";
            }
        }

        public static class Peds
        {
            public static readonly IReadOnlyList<string> CopCityPedModels = new List<string>
            {
                "s_m_y_cop_01",
                "s_f_y_cop_01"
            };

            public static readonly IReadOnlyList<string> CopCountyPedModels = new List<string>
            {
                "s_f_y_sheriff_01",
                "s_m_y_sheriff_01"
            };

            public static readonly IReadOnlyList<string> CopFbiPedModels = new List<string>
            {
                "ig_fbisuit_01",
                "cs_fbisuit_01"
            };

            public static readonly IReadOnlyList<string> CopSwatPedModels = new List<string>
            {
                "s_m_y_swat_01",
            };
            
            /// <summary>
            /// Get a local ped model for the given position.
            /// </summary>
            /// <param name="position">Set the position to get the local model for.</param>
            /// <returns>Returns the local ped model.</returns>
            public static Model GetLocalCop(Vector3 position)
            {
                var zone = GetZone(position);

                return IsCountyZone(zone)
                    ? new Model(Peds.CopCountyPedModels[Random.Next(Peds.CopCountyPedModels.Count)])
                    : new Model(Peds.CopCityPedModels[Random.Next(Peds.CopCityPedModels.Count)]);
            }
            
            public static Model GetPoliceFbiCop()
            {
                return new Model(Peds.CopFbiPedModels[Random.Next(Peds.CopFbiPedModels.Count)]);
            }

            public static Model GetPoliceSwatCop()
            {
                return new Model(Peds.CopSwatPedModels[Random.Next(Peds.CopSwatPedModels.Count)]);
            }

            public static Model GetPoliceBikeCop()
            {
                return new Model(PoliceBikeCopModelName);
            }
        }

        public static class Vehicles
        {
            public static readonly IReadOnlyList<string> CityVehicleModels = new List<string>
            {
                "POLICE",
                "POLICE2",
                PoliceBikeModelName,
                PoliceTransporterModelName
            };

            public static readonly IReadOnlyList<string> CountyVehicleModels = new List<string>
            {
                "SHERIFF",
                "SHERIFF2",
                PoliceBikeModelName,
                PoliceTransporterModelName
            };

            public static readonly IReadOnlyList<string> StateVehicleModels = new List<string>
            {
                "POLICE3",
                PoliceBikeModelName,
                PoliceTransporterModelName
            };

            public static readonly IReadOnlyList<string> FbiVehicleModels = new List<string>
            {
                "FBI",
                "FBI2"
            };

            public static readonly IReadOnlyList<string> SwatVehicleModels = new List<string>
            {
                "Riot"
            };

            /// <summary>
            /// Get a local vehicle model for the given position.
            /// </summary>
            /// <param name="position">Set the position to get the local model for.</param>
            /// <param name="includePoliceBike">Set if the police bike can also be returned as a vehicle model.</param>
            /// <param name="includePoliceTransporter">Set if the police transporter can also be returned as vehicle model.</param>
            /// <returns>Returns the local police vehicle model.</returns>
            public static Model GetLocalPoliceVehicle(Vector3 position, bool includePoliceBike = true, bool includePoliceTransporter = true)
            {
                var zone = GetZone(position);
                var list = IsCountyZone(zone) ? Vehicles.CountyVehicleModels.ToList() : Vehicles.CityVehicleModels.ToList();

                if (!includePoliceBike)
                    list.Remove(PoliceBikeModelName);
                if (!includePoliceTransporter)
                    list.Remove(PoliceTransporterModelName);

                return new Model(list[Random.Next(list.Count)]);
            }
            
            /// <summary>
            /// Get a state police vehicle.
            /// </summary>
            /// <param name="includePoliceBike">Set if the police bike can also be returned as a vehicle model.</param>
            /// <param name="includePoliceTransporter">Set if the police transporter can also be returned as vehicle model.</param>
            /// <returns>Returns a state police vehicle.</returns>
            public static Model GetStatePoliceVehicle(bool includePoliceBike = true, bool includePoliceTransporter = true)
            {
                var list = StateVehicleModels.ToList();

                if (!includePoliceBike)
                    list.Remove(PoliceBikeModelName);
                if (!includePoliceTransporter)
                    list.Remove(PoliceTransporterModelName);

                return new Model(list[Random.Next(list.Count)]);
            }

            /// <summary>
            /// Get an FBI police vehicle model. 
            /// </summary>
            /// <returns>Returns an FBI police vehicle model.</returns>
            public static Model GetFbiPoliceVehicle()
            {
                return new Model(FbiVehicleModels[Random.Next(FbiVehicleModels.Count)]);
            }

            /// <summary>
            /// Get an swat police vehicle model. 
            /// </summary>
            /// <returns>Returns an swat police vehicle model.</returns>
            public static Model GetSwatPoliceVehicle()
            {
                return new Model(SwatVehicleModels[Random.Next(SwatVehicleModels.Count)]);
            }
        }
        
        /// <summary>
        /// Get the medic model.
        /// </summary>
        /// <returns>Returns the medic model.</returns>
        public static Model GetMedic()
        {
            return new Model("s_m_m_paramedic_01");
        }

        /// <summary>
        /// Get the ambulance vehicle model.
        /// </summary>
        /// <returns>Returns the ambulance model.</returns>
        public static Model GetAmbulance()
        {
            return new Model("AMBULANCE");
        }

        /// <summary>
        /// Get the fireman model.
        /// </summary>
        /// <returns>Returns the fireman model.</returns>
        public static Model GetFireman()
        {
            return new Model("s_m_y_fireman_01");
        }

        /// <summary>
        /// Get the firetruck vehicle model.
        /// </summary>
        /// <returns>Returns the firetruck model.</returns>
        public static Model GetFireTruck()
        {
            return new Model("FIRETRUK");
        }

        /// <summary>
        /// Verify if the given model is a bike.
        /// </summary>
        /// <param name="model">The model to verify.</param>
        /// <returns>Returns true if the model is a bike, else false.</returns>
        public static bool IsBike(Model model)
        {
            Assert.NotNull(model, "model cannot be null");
            return model.Name.Equals(PoliceBikeModelName);
        }

        private static bool IsCountyZone(WorldZone zone)
        {
            return zone.County == EWorldZoneCounty.BlaineCounty || zone.County == EWorldZoneCounty.LosSantosCounty;
        }

        private static WorldZone GetZone(Vector3 position)
        {
            return Functions.GetZoneAtPosition(position);
        }
    }
}