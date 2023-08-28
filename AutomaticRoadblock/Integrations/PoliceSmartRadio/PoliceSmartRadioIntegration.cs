using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Logging;
using AutomaticRoadblocks.ManualPlacement;
using AutomaticRoadblocks.Pursuit;
using AutomaticRoadblocks.RedirectTraffic;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Utils;
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

        private readonly ILogger _logger;
        private readonly IPursuitManager _pursuitManager;
        private readonly IManualPlacement _manualPlacement;
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;
        private readonly ISettingsManager _settingsManager;

        public PoliceSmartRadioIntegration(ILogger logger, IPursuitManager pursuitManager, IManualPlacement manualPlacement,
            IRedirectTrafficDispatcher redirectTrafficDispatcher, ISettingsManager settingsManager)
        {
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
            GameUtils.NewSafeFiber(() => _pursuitManager.DispatchNow(true), "PoliceSmartRadioIntegration.DoRoadblockDeployment");
        }

        private void DoManualPlacement()
        {
            GameUtils.NewSafeFiber(
                () => _manualPlacement.PlaceRoadblock(GameUtils.PlayerPosition + MathHelper.ConvertHeadingToDirection(GameUtils.PlayerHeading) *
                    _settingsManager.ManualPlacementSettings.DistanceFromPlayer), "PoliceSmartRadioIntegration.DoManualPlacement");
        }

        private void DoRedirectTraffic()
        {
            GameUtils.NewSafeFiber(
                () => _redirectTrafficDispatcher.DispatchRedirection(GameUtils.PlayerPosition + MathHelper.ConvertHeadingToDirection(GameUtils.PlayerHeading) *
                    _settingsManager.RedirectTrafficSettings.DistanceFromPlayer), "PoliceSmartRadioIntegration.DoRedirectTraffic");
        }

        private void DoRemoveAll()
        {
            GameUtils.NewSafeFiber(() =>
            {
                _manualPlacement.RemoveRoadblocks(RemoveType.All);
                _redirectTrafficDispatcher.RemoveTrafficRedirects(RemoveType.All);
            }, $"{GetType()}.DoRemoveAll");
        }
    }
}