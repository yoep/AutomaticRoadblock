using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficConeDistanceComponentItem : IMenuComponent<UIMenuNumericScrollerItem<double>>
    {
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;

        public RedirectTrafficConeDistanceComponentItem(IRedirectTrafficDispatcher redirectTrafficDispatcher)
        {
            _redirectTrafficDispatcher = redirectTrafficDispatcher;
        }

        /// <inheritdoc />
        public UIMenuNumericScrollerItem<double> MenuItem { get; } = new(
            AutomaticRoadblocksPlugin.RedirectTrafficConeDistance, AutomaticRoadblocksPlugin.RedirectTrafficConeDistanceDescription, 0.2f, 10f, 0.1f);

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