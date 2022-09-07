using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Street;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugRoadInfoComponent : IMenuComponent<UIMenuItem>
    {
        private readonly ILogger _logger;
        private readonly IGame _game;

        public DebugRoadInfoComponent(ILogger logger, IGame game)
        {
            _logger = logger;
            _game = game;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.RoadInfo);
        
        /// <inheritdoc />
        public EMenuType Type => EMenuType.Debug;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            var road = RoadUtils.FindClosestRoad(Game.LocalPlayer.Character.Position, EVehicleNodeType.AllNodes);
            _logger.Info("Nearest road info: " + road);
            _game.DisplayPluginNotification("see console or log file for info about the closest road");
        }
    }
}