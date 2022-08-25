using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class EnableCopsComponentItem  : IMenuComponent<UIMenuCheckboxItem>
    {
        private readonly IManualPlacement _manualPlacement;

        public EnableCopsComponentItem(IManualPlacement manualPlacement)
        {
            _manualPlacement = manualPlacement;
        }

        /// <inheritdoc />
        public UIMenuCheckboxItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.EnableCops, true,
            AutomaticRoadblocksPlugin.EnableCopsDescription);

        /// <inheritdoc />
        public MenuType Type => MenuType.MANUAL_PLACEMENT;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _manualPlacement.CopsEnabled = MenuItem.Checked;
        }

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            MenuItem.Checked = _manualPlacement.CopsEnabled;
            MenuItem.CheckboxEvent += CheckedStateChanged;
        }

        private void CheckedStateChanged(UIMenuCheckboxItem sender, bool @checked)
        {
            _manualPlacement.CopsEnabled = MenuItem.Checked;
        }
    }
}