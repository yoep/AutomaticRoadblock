using System;
using System.Collections.Generic;
using Rage;

namespace AutomaticRoadblocks.Utils
{
    public static class ModelUtils
    {
        private const string RiotModelName = "Riot";

        private static readonly Random Random = new();

        public static class Vehicles
        {
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
    }
}