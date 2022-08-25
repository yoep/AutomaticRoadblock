using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Pursuit.Menu
{
    public class EnableAutomaticLevelIncreaseComponent : IMenuComponent<UIMenuCheckboxItem>
    {
        private readonly IPursuitManager _pursuitManager;

        public EnableAutomaticLevelIncreaseComponent(IPursuitManager pursuitManager)
        {
            _pursuitManager = pursuitManager;
        }

        /// <inheritdoc />
        public UIMenuCheckboxItem MenuItem => new(AutomaticRoadblocksPlugin.EnableAutoPursuitLevelIncrease, true,
            AutomaticRoadblocksPlugin.EnableAutoPursuitLevelIncreaseDescription);

        /// <inheritdoc />
        public MenuType Type => MenuType.PURSUIT;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _pursuitManager.EnableAutomaticLevelIncreases = MenuItem.Checked;
        }

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            MenuItem.Checked = _pursuitManager.EnableAutomaticLevelIncreases;
        }
    }
}