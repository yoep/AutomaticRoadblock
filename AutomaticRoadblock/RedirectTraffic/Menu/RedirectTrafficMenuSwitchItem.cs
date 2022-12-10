using System;
using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Menu.Switcher;
using AutomaticRoadblocks.Utils;
using Rage;
using RAGENativeUI;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficMenuSwitchItem : IMenuSwitchItem, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;
        private readonly ILocalizer _localizer;

        private bool _running = true;

        public RedirectTrafficMenuSwitchItem(ILogger logger,  IRedirectTrafficDispatcher redirectTrafficDispatcher, ILocalizer localizer)
        {
            _logger = logger;
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
            GameUtils.NewSafeFiber(() =>
            {
                while (_running)
                {
                    GameFiber.Yield();
                    DoProcessTick();
                }
            }, "RedirectTrafficMenuSwitchItem.Process");
        }

        private void DoProcessTick()
        {
            try
            {
                if (Menu.Visible)
                {
                    _redirectTrafficDispatcher.CreatePreview();
                }
                else
                {
                    _redirectTrafficDispatcher.DeletePreview();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Traffic redirection preview failed, {ex.Message}", ex);
                GameUtils.DisplayNotification($"~r~{_localizer[LocalizationKey.RedirectTrafficUnknownError]}");
            }
        }
    }
}