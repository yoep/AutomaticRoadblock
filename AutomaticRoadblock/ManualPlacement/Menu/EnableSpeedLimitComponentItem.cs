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
        public MenuType Type => MenuType.MANUAL_PLACEMENT;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _manualPlacement.SpeedLimit = MenuItem.Checked;
        }

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            MenuItem.Checked = _manualPlacement.SpeedLimit;
        }
    }
}