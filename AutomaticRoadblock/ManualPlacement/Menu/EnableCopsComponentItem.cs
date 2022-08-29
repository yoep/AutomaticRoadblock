using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class EnableCopsComponentItem  : IMenuComponent<UIMenuCheckboxItem>
    {
        private readonly IManualPlacement _manualPlacement;

        public EnableCopsComponentItem(IManualPlacement manualPlacement, ILocalizer localizer)
        {
            _manualPlacement = manualPlacement;
            
            MenuItem = new UIMenuCheckboxItem(localizer[LocalizationKey.EnableCops], true,
                localizer[LocalizationKey.EnableCopsDescription]);
        }

        /// <inheritdoc />
        public UIMenuCheckboxItem MenuItem { get; }

        /// <inheritdoc />
        public MenuType Type => MenuType.ManualPlacement;

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