using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class EnableRedirectArrowComponentItem: IMenuComponent<UIMenuCheckboxItem>
    {
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;

        public EnableRedirectArrowComponentItem(IRedirectTrafficDispatcher redirectTrafficDispatcher, ILocalizer localizer)
        {
            _redirectTrafficDispatcher = redirectTrafficDispatcher;

            MenuItem = new UIMenuCheckboxItem(localizer[LocalizationKey.RedirectTrafficEnableRedirectionArrow], true);
        }

        /// <inheritdoc />
        public UIMenuCheckboxItem MenuItem { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.RedirectTraffic;

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
            MenuItem.Checked = _redirectTrafficDispatcher.EnableRedirectionArrow;
            MenuItem.CheckboxEvent += CheckedStateChanged;
        }

        private void CheckedStateChanged(UIMenuCheckboxItem sender, bool @checked)
        {
            _redirectTrafficDispatcher.EnableRedirectionArrow = MenuItem.Checked;
        }
    }
}