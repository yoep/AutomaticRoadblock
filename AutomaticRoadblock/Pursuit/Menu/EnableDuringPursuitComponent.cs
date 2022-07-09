using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Settings;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Pursuit.Menu
{
    public class EnableDuringPursuitComponent : IMenuComponent
    {
        private readonly IPursuitManager _pursuitManager;

        public EnableDuringPursuitComponent(ISettingsManager settingsManager, IPursuitManager pursuitManager)
        {
            _pursuitManager = pursuitManager;
            var enableDuringPursuits = settingsManager.AutomaticRoadblocksSettings.EnableDuringPursuits;

            MenuItem = new UIMenuCheckboxItem(AutomaticRoadblocksPlugin.EnableDuringPursuit, enableDuringPursuits,
                AutomaticRoadblocksPlugin.EnableDuringPursuitDescription);
            _pursuitManager.EnableAutomaticDispatching = enableDuringPursuits;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; }

        /// <inheritdoc />
        public MenuType Type => MenuType.PURSUIT;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _pursuitManager.EnableAutomaticDispatching = ((UIMenuCheckboxItem)MenuItem).Checked;
        }
    }
}