using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Street;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Vehicles;

namespace AutomaticRoadblocks.RedirectTraffic
{
    public class RedirectTrafficDispatcher : AbstractInstancePlacementManager<RedirectTraffic>, IRedirectTrafficDispatcher
    {
        private readonly ISettingsManager _settingsManager;

        private float _coneDistance = 2f;
        private VehicleType _vehicleType = VehicleType.Local;
        private BarrierType _coneType = BarrierType.SmallCone;
        private RedirectTrafficType _type = RedirectTrafficType.Lane;
        private bool _enableRedirectionArrow = true;
        private float _offset;

        public RedirectTrafficDispatcher(IGame game, ILogger logger, ISettingsManager settingsManager)
            : base(game, logger)
        {
            _settingsManager = settingsManager;
        }

        #region Properties

        /// <inheritdoc />
        public float ConeDistance
        {
            get => _coneDistance;
            set => UpdateConeDistance(value);
        }

        /// <inheritdoc />
        public VehicleType VehicleType
        {
            get => _vehicleType;
            set => UpdateVehicleType(value);
        }

        /// <inheritdoc />
        public BarrierType ConeType
        {
            get => _coneType;
            set => UpdateConeType(value);
        }

        /// <inheritdoc />
        public RedirectTrafficType Type
        {
            get => _type;
            set => UpdateType(value);
        }

        /// <inheritdoc />
        public bool EnableRedirectionArrow
        {
            get => _enableRedirectionArrow;
            set => UpdateRedirectArrow(value);
        }

        /// <inheritdoc />
        public float Offset
        {
            get => _offset;
            set => UpdateOffset(value);
        }

        /// <inheritdoc />
        protected override bool IsHologramPreviewEnabled => _settingsManager.RedirectTrafficSettings.EnablePreview;

        /// <inheritdoc />
        protected override float DistanceInFrontOfPlayer => _settingsManager.RedirectTrafficSettings.DistanceFromPlayer;

        #endregion

        #region IRedirectTrafficDispatcher

        /// <inheritdoc />
        public void DispatchRedirection()
        {
            RedirectTraffic redirectTraffic;

            lock (Instances)
            {
                redirectTraffic = Instances.FirstOrDefault(x => x.IsPreviewActive);
                redirectTraffic?.DeletePreview();
            }

            if (redirectTraffic == null)
            {
                redirectTraffic = CreateInstance(LastDeterminedStreet ?? CalculateNewLocationForInstance());

                lock (Instances)
                {
                    Instances.Add(redirectTraffic);
                }
            }

            Game.NewSafeFiber(() =>
            {
                Logger.Trace($"Spawning traffic redirection {redirectTraffic}");
                var success = redirectTraffic.Spawn();

                if (success)
                {
                    Logger.Info($"Traffic redirection has been spawned with success, {redirectTraffic}");
                }
                else
                {
                    Logger.Warn($"Traffic redirection was unable to be spawned correctly, {redirectTraffic}");
                }
            }, "RedirectTrafficDispatcher.DispatchRedirection");
        }

        /// <inheritdoc />
        public void RemoveTrafficRedirects(RemoveType removeType)
        {
            DoInternalInstanceRemoval(removeType);
        }

        #endregion

        #region Function

        protected override RedirectTraffic CreateInstance(IStreet street)
        {
            Logger.Trace(
                $"Creating a redirect traffic instance for {nameof(VehicleType)}: {VehicleType}, {nameof(ConeType)}: {ConeType}, {nameof(Type)}: {Type}, {nameof(ConeDistance)}: {ConeDistance}");
            var redirectTraffic = new RedirectTraffic(new RedirectTraffic.Request
            {
                Road = (Road)street,
                VehicleType = VehicleType,
                ConeType = ConeType,
                Type = Type,
                ConeDistance = ConeDistance,
                EnableRedirectionArrow = EnableRedirectionArrow,
                EnableLights = ShouldAddLights(),
                Offset = Offset
            });
            Logger.Debug($"Created redirect traffic {redirectTraffic}");
            return redirectTraffic;
        }

        private bool ShouldAddLights()
        {
            return _settingsManager.RedirectTrafficSettings.EnableLights &&
                   GameUtils.TimePeriod is ETimePeriod.Evening or ETimePeriod.Night;
        }

        private void UpdateConeDistance(float newDistance)
        {
            _coneDistance = newDistance;
            DoInternalPreviewCreation(true);
        }

        private void UpdateVehicleType(VehicleType value)
        {
            _vehicleType = value;
            DoInternalPreviewCreation(true);
        }

        private void UpdateConeType(BarrierType value)
        {
            _coneType = value;
            DoInternalPreviewCreation(true);
        }

        private void UpdateType(RedirectTrafficType value)
        {
            _type = value;
            DoInternalPreviewCreation(true);
        }


        private void UpdateRedirectArrow(bool value)
        {
            _enableRedirectionArrow = value;
            DoInternalPreviewCreation(true);
        }

        private void UpdateOffset(float offset)
        {
            _offset = offset;
            DoInternalPreviewCreation(true);
        }

        #endregion
    }
}