using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Utils;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugReloadSettingsComponent : IMenuComponent<UIMenuItem>
    {
        private readonly ISettingsManager _settingsManager;

        public DebugReloadSettingsComponent(ISettingsManager settingsManager)
        {
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
            GameUtils.NewSafeFiber(() =>
            {
                _settingsManager.Load();
                GameUtils.DisplayNotification("Settings have been reloaded");
            }, "Debug.ReloadSettings");
        }
    }
}