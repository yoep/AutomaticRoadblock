using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.CloseRoad;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.ManualPlacement;
using AutomaticRoadblocks.RedirectTraffic;
using AutomaticRoadblocks.Roadblock.Dispatcher;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Utils;

namespace AutomaticRoadblocks.ShortKeys
{
    public class CleanAll : ICleanAll
    {
        private const string AudioInstancesCleaned = "ATTENTION_ALL_UNITS WE_ARE_CODE 4";

        private readonly IGame _game;
        private readonly ILogger _logger;
        private readonly ISettingsManager _settingsManager;
        private readonly IRoadblockDispatcher _roadblockDispatcher;
        private readonly IManualPlacement _manualPlacement;
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;
        private readonly ICloseRoadDispatcher _closeRoadDispatcher;

        private bool _active;

        public CleanAll(IGame game, ILogger logger, ISettingsManager settingsManager, IRoadblockDispatcher roadblockDispatcher,
            IManualPlacement manualPlacement, IRedirectTrafficDispatcher redirectTrafficDispatcher, ICloseRoadDispatcher closeRoadDispatcher)
        {
            _game = game;
            _logger = logger;
            _settingsManager = settingsManager;
            _roadblockDispatcher = roadblockDispatcher;
            _manualPlacement = manualPlacement;
            _redirectTrafficDispatcher = redirectTrafficDispatcher;
            _closeRoadDispatcher = closeRoadDispatcher;
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
                        _closeRoadDispatcher.OpenRoads();
                        _logger.Info("Plugin instances have been cleaned");
                        LspdfrUtils.PlayScannerAudioNonBlocking(AudioInstancesCleaned);
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