using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Utils.Road;
using Rage;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug
{
    public class RoadInfo : IMenuComponent
    {
        private readonly ILogger _logger;
        private readonly INotification _notification;

        public RoadInfo(ILogger logger, INotification notification)
        {
            _logger = logger;
            _notification = notification;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AutomaticRoadblocksPlugin.RoadInfo);
        
        /// <inheritdoc />
        public MenuType Type => MenuType.DEBUG;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            var road = RoadUtils.GetClosestRoad(Game.LocalPlayer.Character.Position, RoadType.All);
            _logger.Info("Nearest road info: " + road);
            _notification.DisplayPluginNotification("see console or log file for info about the closest road");
        }
    }
}