using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using LSPD_First_Response.Engine.Scripting;
using LSPD_First_Response.Mod.API;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugZoneInfoComponent : IMenuComponent<UIMenuItem>
    {
        private readonly ILogger _logger;
        private readonly IGame _game;

        public DebugZoneInfoComponent(ILogger logger,IGame game)
        {
            _logger = logger;
            _game = game;
        }

        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.ZoneInfo);
        public EMenuType Type => EMenuType.Debug;
        public bool IsAutoClosed => false;

        public void OnMenuActivation(IMenu sender)
        {
            var zone = Functions.GetZoneAtPosition(_game.PlayerPosition);
            _logger.Info($"Zone info: {FormatZone(zone)}");
            _game.DisplayPluginNotification($"Zone:\n{FormatZone(zone)}");
        }

        private static string FormatZone(WorldZone zone)
        {
            return $"County: {zone.County}\n" +
                   $"Area name: {zone.RealAreaName}\n" +
                   $"Game name: {zone.GameName}";
        }
    }
}