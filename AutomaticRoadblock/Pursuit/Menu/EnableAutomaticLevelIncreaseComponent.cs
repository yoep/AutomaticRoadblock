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
        public UIMenuCheckboxItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.EnableAutoPursuitLevelIncrease, true,
            AutomaticRoadblocksPlugin.EnableAutoPursuitLevelIncreaseDescription);

        /// <inheritdoc />
        public MenuType Type => MenuType.Pursuit;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            // no-op
        }

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            MenuItem.Checked = _pursuitManager.EnableAutomaticLevelIncreases;
            MenuItem.CheckboxEvent += CheckedStateChanged;
        }

        private void CheckedStateChanged(UIMenuCheckboxItem sender, bool @checked)
        {
            _pursuitManager.EnableAutomaticLevelIncreases = MenuItem.Checked;
        }
    }
}