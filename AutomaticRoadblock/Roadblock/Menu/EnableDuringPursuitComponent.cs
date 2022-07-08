using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Settings;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Roadblock.Menu
{
    public class EnableDuringPursuitComponent : IMenuComponent
    {
        public EnableDuringPursuitComponent(ISettingsManager settingsManager)
        {
            MenuItem = new UIMenuCheckboxItem(AutomaticRoadblockPlugin.EnableDuringPursuit, settingsManager.AutomaticRoadblocksSettings.EnableDuringPursuits);
        }

        public UIMenuItem MenuItem { get; private set; }

        /// <inheritdoc />
        public MenuType Type => MenuType.PURSUIT;

        public bool IsAutoClosed { get; }

        public void OnMenuActivation(IMenu sender)
        {
            throw new System.NotImplementedException();
        }
    }
}