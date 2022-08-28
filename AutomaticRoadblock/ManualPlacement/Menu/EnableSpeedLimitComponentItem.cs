using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class EnableSpeedLimitComponentItem: IMenuComponent<UIMenuCheckboxItem>
    {
        private readonly IManualPlacement _manualPlacement;

        public EnableSpeedLimitComponentItem(IManualPlacement manualPlacement)
        {
            _manualPlacement = manualPlacement;
        }

        /// <inheritdoc />
        public UIMenuCheckboxItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.SpeedLimit, true,
            AutomaticRoadblocksPlugin.SpeedLimitDescription);

        /// <inheritdoc />
        public MenuType Type => MenuType.ManualPlacement;

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
            MenuItem.Checked = _manualPlacement.SpeedLimit;
            MenuItem.CheckboxEvent += CheckedStateChanged;
        }

        private void CheckedStateChanged(UIMenuCheckboxItem sender, bool @checked)
        {
            _manualPlacement.SpeedLimit = MenuItem.Checked;
        }
    }
}