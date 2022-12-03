using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.ManualPlacement;
using AutomaticRoadblocks.Pursuit;
using AutomaticRoadblocks.RedirectTraffic;
using AutomaticRoadblocks.Settings;
using PoliceSmartRadio.API;
using Rage;

namespace AutomaticRoadblocks.Integrations.PoliceSmartRadio
{
    /// <summary>
    /// The implementation for <see cref="IPoliceSmartRadio"/> which actually integrates with police smart radio.
    /// </summary>
    public class PoliceSmartRadioIntegration : IPoliceSmartRadio
    {
        private const string RoadblockButtonName = "roadblocks";
        private const string ManualPlacementButtonName = "manualplacement";
        private const string RedirectTrafficButtonName = "redirecttraffic";
        private const string RemoveAllButtonName = "removeall";

        private readonly IGame _game;
        private readonly ILogger _logger;
        private readonly IPursuitManager _pursuitManager;
        private readonly IManualPlacement _manualPlacement;
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;
        private readonly ISettingsManager _settingsManager;

        public PoliceSmartRadioIntegration(IGame game, ILogger logger, IPursuitManager pursuitManager, IManualPlacement manualPlacement,
            IRedirectTrafficDispatcher redirectTrafficDispatcher, ISettingsManager settingsManager)
        {
            _game = game;
            _logger = logger;
            _pursuitManager = pursuitManager;
            _manualPlacement = manualPlacement;
            _redirectTrafficDispatcher = redirectTrafficDispatcher;
            _settingsManager = settingsManager;
        }

        /// <inheritdoc />
        public void Initialize()
        {
            Functions.AddActionToButton(DoRoadblockDeployment, () => _pursuitManager.IsPursuitActive, RoadblockButtonName);
            Functions.AddActionToButton(DoManualPlacement, ManualPlacementButtonName);
            Functions.AddActionToButton(DoRedirectTraffic, RedirectTrafficButtonName);
            Functions.AddActionToButton(DoRemoveAll, RemoveAllButtonName);
            _logger.Info("Initialized Police SmartRadio integration");
        }

        private void DoRoadblockDeployment()
        {
            _game.NewSafeFiber(() => _pursuitManager.DispatchNow(true), "PoliceSmartRadioIntegration.DoRoadblockDeployment");
        }

        private void DoManualPlacement()
        {
            _game.NewSafeFiber(
                () => _manualPlacement.PlaceRoadblock(_game.PlayerPosition + MathHelper.ConvertHeadingToDirection(_game.PlayerHeading) *
                    _settingsManager.ManualPlacementSettings.DistanceFromPlayer), "PoliceSmartRadioIntegration.DoManualPlacement");
        }

        private void DoRedirectTraffic()
        {
            _game.NewSafeFiber(
                () => _redirectTrafficDispatcher.DispatchRedirection(_game.PlayerPosition + MathHelper.ConvertHeadingToDirection(_game.PlayerHeading) *
                    _settingsManager.RedirectTrafficSettings.DistanceFromPlayer), "PoliceSmartRadioIntegration.DoRedirectTraffic");
        }

        private void DoRemoveAll()
        {
            _game.NewSafeFiber(() =>
            {
                _manualPlacement.RemoveRoadblocks(RemoveType.All);
                _redirectTrafficDispatcher.RemoveTrafficRedirects(RemoveType.All);
            }, "PoliceSmartRadioIntegration.DoRemoveAll");
        }
    }
}