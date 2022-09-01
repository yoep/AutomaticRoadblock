using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Pursuit.Menu
{
    public class PursuitEnableDuringPursuitComponentItem : IMenuComponent<UIMenuCheckboxItem>
    {
        private readonly IPursuitManager _pursuitManager;

        public PursuitEnableDuringPursuitComponentItem(IPursuitManager pursuitManager, ILocalizer localizer)
        {
            _pursuitManager = pursuitManager;

            MenuItem = new UIMenuCheckboxItem(localizer[LocalizationKey.EnableDuringPursuit], true,
                localizer[LocalizationKey.EnableDuringPursuitDescription]);
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
            MenuItem.Checked = _pursuitManager.EnableAutomaticDispatching;
            MenuItem.CheckboxEvent += CheckedStateChanged;
        }

        private void CheckedStateChanged(UIMenuCheckboxItem sender, bool @checked)
        {
            _pursuitManager.EnableAutomaticDispatching = MenuItem.Checked;
        }
    }
}