using System;
using System.Linq;
using AutomaticRoadblocks.Logging;
using AutomaticRoadblocks.Menu;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugCleanEntitiesComponent : IMenuComponent<UIMenuItem>
    {
        private readonly ILogger _logger;

        public DebugCleanEntitiesComponent(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.RemoveEntities, AutomaticRoadblocksPlugin.RemoveEntitiesDescription);

        /// <inheritdoc />
        public EMenuType Type => EMenuType.Debug;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            try
            {
                var localPlayer = Game.LocalPlayer;
                
                _logger.Info("Cleaning all entities...");
                World.GetEntities(GetEntitiesFlags.ConsiderAllObjects | GetEntitiesFlags.ConsiderAllPeds | GetEntitiesFlags.ConsiderAllVehicles)
                    .Where(x => x != localPlayer.Character && x != localPlayer.LastVehicle)
                    .ToList()
                    .ForEach(ForceDelete);
                
                _logger.Info("Cleaning all blips...");
                foreach (var blip in World.GetAllBlips())
                {
                    blip.Delete();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to clean world entities, {ex.Message}", ex);
            }
        }

        private static void ForceDelete(Entity entity)
        {
            if (entity != null && entity.IsValid())
            {
                entity.Dismiss();
                entity.Delete();
            }
        }
    }
}