using AutomaticRoadblocks.Vehicles;

namespace AutomaticRoadblocks.RedirectTraffic
{
    public class RedirectTrafficDispatcher : IRedirectTrafficDispatcher
    {
        private float _coneDistance = 2f;
        private VehicleType _vehicleType = VehicleType.Locale;

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

        #endregion

        #region IRedirectTrafficDispatcher

        /// <inheritdoc />
        public void Dispatch()
        {
        }

        /// <inheritdoc />
        public void CreatePreview(bool force = false)
        {
            
        }

        /// <inheritdoc />
        public void RemovePreviews()
        {
            
        }

        #endregion

        #region Function

        private void UpdateConeDistance(float newDistance)
        {
            _coneDistance = newDistance;
            CreatePreview(true);
        }

        private void UpdateVehicleType(VehicleType value)
        {
            _vehicleType = value;
            CreatePreview(true);
        }

        #endregion
    }
}