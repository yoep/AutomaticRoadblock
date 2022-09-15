using System;
using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Menu.Switcher;
using RAGENativeUI;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficMenuSwitchItem : IMenuSwitchItem, IDisposable
    {
        private readonly IGame _game;
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;
        private readonly ILocalizer _localizer;

        private bool _running = true;

        public RedirectTrafficMenuSwitchItem(IGame game, IRedirectTrafficDispatcher redirectTrafficDispatcher, ILocalizer localizer)
        {
            _game = game;
            _redirectTrafficDispatcher = redirectTrafficDispatcher;
            _localizer = localizer;

            Menu = new UIMenu(_localizer[LocalizationKey.MenuTitle],
                "~b~" + _localizer[LocalizationKey.MenuSubtitle]);
        }

        /// <inheritdoc />
        public UIMenu Menu { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.RedirectTraffic;

        /// <inheritdoc />
        public string DisplayText => _localizer[LocalizationKey.MenuRedirectTraffic];

        /// <inheritdoc />
        public void Dispose()
        {
            _running = false;
        }

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            Process();
        }

        private void Process()
        {
            _game.NewSafeFiber(() =>
            {
                while (_running)
                {
                    _game.FiberYield();

                    if (Menu.Visible)
                    {
                        _redirectTrafficDispatcher.CreatePreview();
                    }
                    else
                    {
                        _redirectTrafficDispatcher.DeletePreview();
                    }
                }
            }, "RedirectTrafficMenuSwitchItem.Process");
        }
    }
}