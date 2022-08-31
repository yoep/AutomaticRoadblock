using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
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
                redirectTraffic = CreateInstance(LastDeterminedRoad ?? CalculateNewLocationForInstance());
                Logger.Info($"Created redirect traffic {redirectTraffic}");

                lock (Instances)
                {
                    Instances.Add(redirectTraffic);
                }
            }

            Game.NewSafeFiber(() => { redirectTraffic.Spawn(); }, "RedirectTrafficDispatcher.DispatchRedirection");
        }

        /// <inheritdoc />
        public void RemoveTrafficRedirects(RemoveType removeType)
        {
            DoInternalInstanceRemoval(removeType);
        }

        #endregion

        #region Function

        protected override RedirectTraffic CreateInstance(Road road)
        {
            Logger.Debug(
                $"Creating a redirect traffic instance for {nameof(VehicleType)}: {VehicleType}, {nameof(ConeType)}: {ConeType}, {nameof(Type)}: {Type}, {nameof(ConeDistance)}: {ConeDistance}");
            return new RedirectTraffic(new RedirectTraffic.Request
            {
                Road = road,
                VehicleType = VehicleType,
                ConeType = ConeType,
                Type = Type,
                ConeDistance = ConeDistance,
                EnableRedirectionArrow = EnableRedirectionArrow,
                EnableLights = ShouldAddLights(),
                Offset = Offset
            });
        }

        private bool ShouldAddLights()
        {
            return _settingsManager.RedirectTrafficSettings.EnableLights &&
                   GameUtils.TimePeriod is TimePeriod.Evening or TimePeriod.Night;
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