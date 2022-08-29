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
        
        private bool _running = true;

        public RedirectTrafficMenuSwitchItem(IGame game, IRedirectTrafficDispatcher redirectTrafficDispatcher)
        {
            _game = game;
            _redirectTrafficDispatcher = redirectTrafficDispatcher;
        }

        /// <inheritdoc />
        public UIMenu Menu { get; } = new(IoC.Instance.GetInstance<ILocalizer>()[LocalizationKey.MenuTitle],
            "~b~" + IoC.Instance.GetInstance<ILocalizer>()[LocalizationKey.MenuSubtitle]);

        /// <inheritdoc />
        public MenuType Type => MenuType.RedirectTraffic;

        /// <inheritdoc />
        public string DisplayText => AutomaticRoadblocksPlugin.MenuRedirectTraffic;
        
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