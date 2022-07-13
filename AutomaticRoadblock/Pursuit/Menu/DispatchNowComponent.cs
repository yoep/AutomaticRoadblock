using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Pursuit.Menu
{
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
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _pursuitManager.DispatchNow();
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