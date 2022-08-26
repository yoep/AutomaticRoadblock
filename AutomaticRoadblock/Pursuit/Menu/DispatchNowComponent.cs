using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Pursuit.Menu
{
    /// <summary>
    /// Dispatch now menu item for when the user is in a pursuit.
    /// </summary>
    public class DispatchNowComponent : IMenuComponent<UIMenuItem>
    {
        private readonly IPursuitManager _pursuitManager;

        public DispatchNowComponent(IPursuitManager pursuitManager)
        {
            _pursuitManager = pursuitManager;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.DispatchNow, AutomaticRoadblocksPlugin.DispatchNowDescription);

        /// <inheritdoc />
        public MenuType Type => MenuType.PURSUIT;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _pursuitManager.DispatchNow(true);
        }

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            MenuItem.Enabled = false;
            _pursuitManager.PursuitStateChanged += PursuitStateChanged;
        }

        private void PursuitStateChanged(bool isPursuitActive)
        {
            MenuItem.Enabled = isPursuitActive;
        }
    }
}