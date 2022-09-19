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
        private const string RiotModelName = "Riot";

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
                    ? new Model(CopCountyPedModels[Random.Next(CopCountyPedModels.Count)])
                    : new Model(CopCityPedModels[Random.Next(CopCityPedModels.Count)]);
            }

            public static Model GetPoliceFbiCop()
            {
                return new Model(CopFbiPedModels[Random.Next(CopFbiPedModels.Count)]);
            }

            public static Model GetPoliceSwatCop()
            {
                return new Model(CopSwatPedModels[Random.Next(CopSwatPedModels.Count)]);
            }

            public static Model GetPoliceBikeCop()
            {
                return new Model(PoliceBikeCopModelName);
            }

            /// <summary>
            /// Verify if the given model is an FBI cop.
            /// </summary>
            /// <param name="model">The model to check.</param>
            /// <returns>Returns true if the model is an FBI cop.</returns>
            public static bool IsFbi(Model model)
            {
                Assert.NotNull(model, "model cannot be null");
                return CopFbiPedModels.Contains(model.Name);
            }

            /// <summary>
            /// Verify if the given model is an swat unit.
            /// </summary>
            /// <param name="model">The model to check.</param>
            /// <returns>Returns true if the model is an swat unit.</returns>
            public static bool IsSwat(Model model)
            {
                Assert.NotNull(model, "model cannot be null");
                return CopSwatPedModels.Contains(model.Name);
            }
        }

        public static class Vehicles
        {
            public static readonly IReadOnlyList<string> CityVehicleModels = new List<string>
            {
                "POLICE",
                "POLICE2"
            };

            public static readonly IReadOnlyList<string> CountyVehicleModels = new List<string>
            {
                "SHERIFF",
                "SHERIFF2"
            };

            public static readonly IReadOnlyList<string> StateVehicleModels = new List<string>
            {
                "POLICE3",
                PoliceBikeModelName,
            };

            public static readonly IReadOnlyList<string> FbiVehicleModels = new List<string>
            {
                "FBI",
                "FBI2"
            };

            public static readonly IReadOnlyList<string> RaceVehicleModels = new List<string>
            {
                "penumbra2",
                "coquette4",
                "sugoi",
                "sultan2",
                "imorgon",
                "komoda",
                "jugular",
                "neo",
                "issi7",
                "drafter",
                "paragon2",
                "italigto",
            };

            /// <summary>
            /// Get a race vehicle model. 
            /// </summary>
            /// <returns>Returns a race vehicle model.</returns>
            public static Model GetRaceVehicle()
            {
                return new Model(RaceVehicleModels[Random.Next(RaceVehicleModels.Count)]);
            }

            /// <summary>
            /// Verify if the given vehicle model is a transporter.
            /// </summary>
            /// <param name="model">The model to verify.</param>
            /// <returns>Returns true if transporter, else false.</returns>
            public static bool IsTransporter(Model model)
            {
                Assert.NotNull(model, "model cannot be null");
                return model.Name.Equals(PoliceTransporterModelName);
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

            /// <summary>
            /// Verify if the given model is a riot vehicle.
            /// </summary>
            /// <param name="model">The model to verify.</param>
            /// <returns>Returns true if the model is a riot, else false.</returns>
            public static bool IsRiot(Model model)
            {
                Assert.NotNull(model, "model cannot be null");
                return model.Name.Equals(RiotModelName);
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

        private static bool IsCountyZone(WorldZone zone)
        {
            return zone.County is EWorldZoneCounty.BlaineCounty or EWorldZoneCounty.LosSantosCounty;
        }

        private static WorldZone GetZone(Vector3 position)
        {
            return Functions.GetZoneAtPosition(position);
        }
    }
}