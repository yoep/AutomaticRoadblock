using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.ManualPlacement;
using AutomaticRoadblocks.RedirectTraffic;
using AutomaticRoadblocks.Roadblock.Dispatcher;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Utils;

namespace AutomaticRoadblocks.ShortKeys
{
    public class CleanAll : ICleanAll
    {
        private readonly IGame _game;
        private readonly ILogger _logger;
        private readonly ISettingsManager _settingsManager;
        private readonly ILocalizer _localizer;
        private readonly IRoadblockDispatcher _roadblockDispatcher;
        private readonly IManualPlacement _manualPlacement;
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;

        private bool _active;

        public CleanAll(IGame game, ILogger logger, ISettingsManager settingsManager, ILocalizer localizer, IRoadblockDispatcher roadblockDispatcher,
            IManualPlacement manualPlacement, IRedirectTrafficDispatcher redirectTrafficDispatcher)
        {
            _game = game;
            _logger = logger;
            _settingsManager = settingsManager;
            _localizer = localizer;
            _roadblockDispatcher = roadblockDispatcher;
            _manualPlacement = manualPlacement;
            _redirectTrafficDispatcher = redirectTrafficDispatcher;
        }

        /// <inheritdoc />
        public void OnDutyStarted()
        {
            _active = true;
            _game.NewSafeFiber(() =>
            {
                var settings = _settingsManager.GeneralSettings;
                _logger.Trace($"Starting clean all fiber with keys {settings.CleanAllKey} & {settings.CleanAllModifierKey}");

                while (_active)
                {
                    _game.FiberYield();
                    
                    if (GameUtils.IsKeyPressed(settings.CleanAllKey, settings.CleanAllModifierKey))
                    {
                        _logger.Debug("Cleaning all plugin instances");
                        _roadblockDispatcher.DismissActiveRoadblocks();
                        _manualPlacement.RemoveRoadblocks(RemoveType.All);
                        _redirectTrafficDispatcher.RemoveTrafficRedirects(RemoveType.All);
                        _logger.Info("Plugin instances have been cleaned");
                        _game.DisplayNotification(_localizer[LocalizationKey.InstancesCleaned]);
                    }
                }
                
                _logger.Trace("Clean all fiber has been stopped");
            }, "CleanAll.Listener");
        }

        /// <inheritdoc />
        public void OnDutyEnded()
        {
            _active = false;
        }
    }
}