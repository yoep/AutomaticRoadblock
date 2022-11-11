using System;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
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
                World.GetEntities(GetEntitiesFlags.ConsiderAllObjects | GetEntitiesFlags.ConsiderAllPeds | GetEntitiesFlags.ConsiderAllVehicles)
                    .Where(x => x != localPlayer.Character && x != localPlayer.LastVehicle)
                    .ToList()
                    .ForEach(x => x.Dismiss());
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to clean world entities, {ex.Message}", ex);
            }
        }
    }
}