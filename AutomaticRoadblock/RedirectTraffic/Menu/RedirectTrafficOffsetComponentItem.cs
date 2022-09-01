using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficOffsetComponentItem : IMenuComponent<UIMenuNumericScrollerItem<double>>
    {
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;

        public RedirectTrafficOffsetComponentItem(IRedirectTrafficDispatcher redirectTrafficDispatcher, ILocalizer localizer)
        {
            _redirectTrafficDispatcher = redirectTrafficDispatcher;

            MenuItem = new UIMenuNumericScrollerItem<double>(
                localizer[LocalizationKey.Offset], localizer[LocalizationKey.OffsetDescription], -10f, 10f, 0.1f);
        }

        /// <inheritdoc />
        public UIMenuNumericScrollerItem<double> MenuItem { get; }

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
            MenuItem.Value = _redirectTrafficDispatcher.Offset;
            MenuItem.IndexChanged += ValueChanged;
        }

        private void ValueChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _redirectTrafficDispatcher.Offset = (float)MenuItem.Value;
        }
    }
}