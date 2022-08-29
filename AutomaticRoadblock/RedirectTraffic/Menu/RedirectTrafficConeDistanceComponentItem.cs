using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficConeDistanceComponentItem : IMenuComponent<UIMenuNumericScrollerItem<double>>
    {
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;

        public RedirectTrafficConeDistanceComponentItem(IRedirectTrafficDispatcher redirectTrafficDispatcher, ILocalizer localizer)
        {
            _redirectTrafficDispatcher = redirectTrafficDispatcher;

            MenuItem = new UIMenuNumericScrollerItem<double>(
                localizer[LocalizationKey.RedirectTrafficConeDistance], localizer[LocalizationKey.RedirectTrafficConeDistanceDescription], 0.5f, 30f, 0.5f);
        }

        /// <inheritdoc />
        public UIMenuNumericScrollerItem<double> MenuItem { get; }

        /// <inheritdoc />
        public MenuType Type => MenuType.RedirectTraffic;

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
            MenuItem.Value = _redirectTrafficDispatcher.ConeDistance;
            MenuItem.IndexChanged += ValueChanged;
        }

        private void ValueChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _redirectTrafficDispatcher.ConeDistance = (float)MenuItem.Value;
        }
    }
}