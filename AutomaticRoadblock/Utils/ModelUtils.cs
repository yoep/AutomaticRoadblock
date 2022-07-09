using System;
using System.Collections.Generic;
using LSPD_First_Response.Engine.Scripting;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AutomaticRoadblocks.Utils
{
    public static class ModelUtils
    {
        private const string PoliceBikeModelName = "POLICEB";
        private static readonly Random Random = new Random();

        #region Constants

        public static class Weapons
        {
            public const string Pistol = "weapon_pistol";
            public const string StunGun = "weapon_stungun_mp";
        }

        public static class Peds
        {
            public static readonly List<string> CopCityPedModels = new List<string>
            {
                "s_m_y_cop_01",
                "s_f_y_cop_01"
            };

            public static readonly List<string> CopCountyPedModels = new List<string>
            {
                "s_f_y_sheriff_01",
                "s_m_y_sheriff_01"
            };

            public static readonly List<string> CopStatePedModels = new List<string>
            {
                "s_m_y_hwaycop_01"
            };

            public static readonly List<string> CopFbiPedModels = new List<string>
            {
                "ig_fbisuit_01",
                "cs_fbisuit_01"
            };

            public static readonly List<string> CopSwatPedModels = new List<string>
            {
                "s_m_y_swat_01",
            };
        }

        internal static readonly List<string> CityVehicleModels = new List<string>
        {
            "POLICE",
            "POLICE2",
            PoliceBikeModelName
        };

        internal static readonly List<string> CountyVehicleModels = new List<string>
        {
            "SHERIFF",
            "SHERIFF2",
            PoliceBikeModelName
        };

        internal static readonly List<string> StateVehicleModels = new List<string>
        {
            "POLICE3",
            PoliceBikeModelName
        };

        internal static readonly List<string> FbiVehicleModels = new List<string>
        {
            "FBI",
            "FBI2"
        };

        internal static readonly List<string> SwatVehicleModels = new List<string>
        {
            "Riot"
        };

        #endregion

        #region Methods

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

        /// <summary>
        /// Get a local vehicle model for the given position.
        /// </summary>
        /// <param name="position">Set the position to get the local model for.</param>
        /// <returns>Returns the local vehicle model.</returns>
        public static Model GetLocalPolice(Vector3 position)
        {
            var zone = GetZone(position);

            return IsCountyZone(zone)
                ? new Model(CountyVehicleModels[Random.Next(CountyVehicleModels.Count)])
                : new Model(CityVehicleModels[Random.Next(CityVehicleModels.Count)]);
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

        #endregion

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