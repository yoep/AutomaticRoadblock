using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Utils;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Pursuit.Menu
{
    /// <summary>
    /// Dispatch now menu item for when the user is in a pursuit.
    /// </summary>
    public class PursuitDispatchNowComponent : IMenuComponent<UIMenuItem>
    {
        private readonly IPursuitManager _pursuitManager;

        public PursuitDispatchNowComponent(IPursuitManager pursuitManager,  ILocalizer localizer)
        {
            _pursuitManager = pursuitManager;

            MenuItem = new UIMenuItem(localizer[LocalizationKey.DispatchNow],
                localizer[LocalizationKey.DispatchNowDescription]);
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.Pursuit;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            GameUtils.NewSafeFiber(() => _pursuitManager.DispatchNow(true), "DispatchNowComponent.OnMenuActivation");
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