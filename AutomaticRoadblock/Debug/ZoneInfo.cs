using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using LSPD_First_Response.Engine.Scripting;
using LSPD_First_Response.Mod.API;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug
{
    public class ZoneInfo : IMenuComponent
    {
        private readonly ILogger _logger;
        private readonly INotification _notification;
        private readonly IGame _game;

        public ZoneInfo(ILogger logger, INotification notification, IGame game)
        {
            _logger = logger;
            _notification = notification;
            _game = game;
        }

        public UIMenuItem MenuItem { get; } = new UIMenuItem(AutomaticRoadblocksPlugin.ZoneInfo);
        public MenuType Type => MenuType.DEBUG;
        public bool IsAutoClosed => false;
        public void OnMenuActivation(IMenu sender)
        {
            var zone = Functions.GetZoneAtPosition(_game.PlayerPosition);
            _logger.Info($"Zone info: {FormatZone(zone)}");
            _notification.DisplayPluginNotification($"Zone:\n{FormatZone(zone)}");
        }

        private static string FormatZone(WorldZone zone)
        {
            return $"County: {zone.County}\n" +
                   $"Area name: {zone.RealAreaName}\n" +
                   $"Game name: {zone.GameName}";
        }
    }
}