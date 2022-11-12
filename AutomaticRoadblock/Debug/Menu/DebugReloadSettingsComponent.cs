using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Settings;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugReloadSettingsComponent : IMenuComponent<UIMenuItem>
    {
        private readonly IGame _game;
        private readonly ISettingsManager _settingsManager;

        public DebugReloadSettingsComponent(IGame game, ISettingsManager settingsManager)
        {
            _game = game;
            _settingsManager = settingsManager;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.ReloadSettings, AutomaticRoadblocksPlugin.ReloadSettingsDescription);

        /// <inheritdoc />
        public EMenuType Type => EMenuType.Debug;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _game.NewSafeFiber(() =>
            {
                _settingsManager.Load();
                _game.DisplayNotification("Settings have been reloaded");
            }, "Debug.ReloadSettings");
        }
    }
}