using System;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.SpikeStrip
{
    /// <summary>
    /// The pursuit spike strip is responsible for detecting if the target vehicle has been hit with the spike strip.
    /// Next to the detection system, it will also play the audio effects for the spike strip during a pursuit.
    /// It extends upon the basic functionality of <see cref="SpikeStrip"/>.
    /// </summary>
    public class PursuitSpikeStrip : SpikeStrip
    {
        private const float BypassTolerance = 20f;
        private const string AudioSpikeStripDeployedLeft = "ROADBLOCK_SPIKESTRIP_DEPLOYED_LEFT";
        private const string AudioSpikeStripDeployedMiddle = "ROADBLOCK_SPIKESTRIP_DEPLOYED_MIDDLE";
        private const string AudioSpikeStripDeployedRight = "ROADBLOCK_SPIKESTRIP_DEPLOYED_RIGHT";

        private float _lastKnownDistanceToSpikeStrip = 9999f;

        public PursuitSpikeStrip(Road street, ESpikeStripLocation location, Vehicle targetVehicle, float offset)
            : base(street, location, offset)
        {
            Assert.NotNull(targetVehicle, "targetVehicle cannot be null");
            TargetVehicle = targetVehicle;
            StateChanged += SpikeStripStateChanged;
        }

        internal PursuitSpikeStrip(Road street, Road.Lane lane, ESpikeStripLocation location, Vehicle targetVehicle, float offset)
            : base(street, lane, location, offset)
        {
            Assert.NotNull(targetVehicle, "targetVehicle cannot be null");
            TargetVehicle = targetVehicle;
            StateChanged += SpikeStripStateChanged;
        }

        #region Properties

        /// <summary>
        /// The target vehicle this spike strip tries to target.
        /// </summary>
        private Vehicle TargetVehicle { get; }

        private bool IsVehicleInstanceInvalid => TargetVehicle == null || !TargetVehicle.IsValid();

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Type: {nameof(PursuitSpikeStrip)}, {base.ToString()}";
        }

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override void VehicleHitSpikeStrip(Vehicle vehicle, VehicleWheel wheel)
        {
            base.VehicleHitSpikeStrip(vehicle, wheel);

            if (vehicle == TargetVehicle)
            {
                Logger.Info("Suspect vehicle hit the spike strip");
                UpdateState(ESpikeStripState.Hit);
            }
        }

        /// <inheritdoc />
        protected override void DoAdditionalVerifications()
        {
            if (IsVehicleInstanceInvalid || State is not ESpikeStripState.Deploying or ESpikeStripState.Deployed)
                return;

            var currentDistance = TargetVehicle.DistanceTo(Position);

            if (currentDistance < _lastKnownDistanceToSpikeStrip)
            {
                _lastKnownDistanceToSpikeStrip = currentDistance;
            }
            else if (Math.Abs(currentDistance - _lastKnownDistanceToSpikeStrip) > BypassTolerance)
            {
                UpdateState(ESpikeStripState.Bypassed);
                Logger.Info("Spike strip has been bypassed");
            }
        }

        private void SpikeStripStateChanged(ISpikeStrip spikeStrip, ESpikeStripState state)
        {
            switch (state)
            {
                case ESpikeStripState.Deploying:
                    LspdfrUtils.PlayScannerAudio(GetAudioName(spikeStrip.Location));
                    break;
            }
        }

        private static string GetAudioName(ESpikeStripLocation stripLocation)
        {
            return stripLocation switch
            {
                ESpikeStripLocation.Left => AudioSpikeStripDeployedLeft,
                ESpikeStripLocation.Middle => AudioSpikeStripDeployedMiddle,
                _ => AudioSpikeStripDeployedRight
            };
        }

        #endregion
    }
}