using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Pursuit.Menu
{
    /// <summary>
    /// Dispatch now menu item for when the user is in a pursuit.
    /// </summary>
    public class DispatchNowComponent : IMenuComponent<UIMenuItem>
    {
        private readonly IGame _game;
        private readonly IPursuitManager _pursuitManager;

        public DispatchNowComponent(IPursuitManager pursuitManager, IGame game)
        {
            _pursuitManager = pursuitManager;
            _game = game;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.DispatchNow, AutomaticRoadblocksPlugin.DispatchNowDescription);

        /// <inheritdoc />
        public MenuType Type => MenuType.Pursuit;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _game.NewSafeFiber(() => _pursuitManager.DispatchNow(true), "DispatchNowComponent.OnMenuActivation");
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