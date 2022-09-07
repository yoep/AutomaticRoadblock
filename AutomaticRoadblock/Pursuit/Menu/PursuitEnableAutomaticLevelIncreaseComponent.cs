using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Pursuit.Menu
{
    public class PursuitEnableAutomaticLevelIncreaseComponent : IMenuComponent<UIMenuCheckboxItem>
    {
        private readonly IPursuitManager _pursuitManager;

        public PursuitEnableAutomaticLevelIncreaseComponent(IPursuitManager pursuitManager, ILocalizer localizer)
        {
            _pursuitManager = pursuitManager;

            MenuItem = new UIMenuCheckboxItem(localizer[LocalizationKey.EnableAutoPursuitLevelIncrease], true,
                localizer[LocalizationKey.EnableAutoPursuitLevelIncreaseDescription]);
        }

        /// <inheritdoc />
        public UIMenuCheckboxItem MenuItem { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.Pursuit;

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