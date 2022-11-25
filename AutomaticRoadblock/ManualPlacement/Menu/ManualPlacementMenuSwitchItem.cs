using System;
using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Menu.Switcher;
using RAGENativeUI;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualPlacementMenuSwitchItem : IMenuSwitchItem, IDisposable
    {
        private readonly IGame _game;
        private readonly ILogger _logger;
        private readonly IManualPlacement _manualPlacement;
        private readonly ILocalizer _localizer;

        private bool _running = true;

        public ManualPlacementMenuSwitchItem(IGame game, ILogger logger, IManualPlacement manualPlacement, ILocalizer localizer)
        {
            _game = game;
            _logger = logger;
            _manualPlacement = manualPlacement;
            _localizer = localizer;

            Menu = new UIMenu(_localizer[LocalizationKey.MenuTitle],
                "~b~" + _localizer[LocalizationKey.MenuSubtitle]);
        }

        /// <inheritdoc />
        public UIMenu Menu { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.ManualPlacement;

        /// <inheritdoc />
        public string DisplayText => _localizer[LocalizationKey.MenuManualPlacement];

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
                    DoPreviewTick();
                }
            }, "ManualPlacementMenuSwitchItem.Process");
        }

        private void DoPreviewTick()
        {
            try
            {
                if (Menu.Visible)
                {
                    _manualPlacement.CreatePreview();
                }
                else
                {
                    _manualPlacement.DeletePreview();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Manual placement preview failed, {ex.Message}", ex);
                _game.DisplayNotification("~r~An unexpected error occurred while handling the manual roadblock placement");
            }
        }
    }
}