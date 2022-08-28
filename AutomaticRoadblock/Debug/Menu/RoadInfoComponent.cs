using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Utils.Road;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class RoadInfoComponent : IMenuComponent<UIMenuItem>
    {
        private readonly ILogger _logger;
        private readonly IGame _game;

        public RoadInfoComponent(ILogger logger, IGame game)
        {
            _logger = logger;
            _game = game;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AutomaticRoadblocksPlugin.RoadInfo);
        
        /// <inheritdoc />
        public MenuType Type => MenuType.Debug;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            var road = RoadUtils.FindClosestRoad(Game.LocalPlayer.Character.Position, RoadType.All);
            _logger.Info("Nearest road info: " + road);
            _game.DisplayPluginNotification("see console or log file for info about the closest road");
        }
    }
}