using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Vehicles;

namespace AutomaticRoadblocks.RedirectTraffic
{
    public class RedirectTrafficDispatcher : AbstractInstancePlacementManager<RedirectTraffic>, IRedirectTrafficDispatcher
    {
        private readonly ISettingsManager _settingsManager;

        private float _coneDistance = 2f;
        private VehicleType _vehicleType = VehicleType.Locale;
        private BarrierType _coneType = BarrierType.SmallCone;
        private RedirectTrafficType _type = RedirectTrafficType.Lane;

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
        protected override bool IsHologramPreviewEnabled => _settingsManager.RedirectTrafficSettings.EnablePreview;

        /// <inheritdoc />
        protected override float DistanceInFrontOfPlayer => 10f;

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
            return new RedirectTraffic(road, VehicleType, ConeType, Type, ConeDistance);
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

        #endregion
    }
}