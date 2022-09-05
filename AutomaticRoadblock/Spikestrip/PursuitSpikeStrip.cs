using System;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.SpikeStrip
{
    /// <summary>
    /// The pursuit spike strip is responsible for detecting if the target vehicle has been hit with the spike strip.
    /// It extends upon the basic functionality of <see cref="SpikeStrip"/>.
    /// </summary>
    public class PursuitSpikeStrip : SpikeStrip
    {
        private const float BypassTolerance = 20f;
        
        private float _lastKnownDistanceToSpikeStrip = 9999f;
        
        public PursuitSpikeStrip(Road road, ESpikeStripLocation location, Vehicle targetVehicle) 
            : base(road, location)
        {
            Assert.NotNull(targetVehicle, "targetVehicle cannot be null");
            TargetVehicle = targetVehicle;
        }

        internal PursuitSpikeStrip(Road road, Road.Lane lane, ESpikeStripLocation location, Vehicle targetVehicle) 
            : base(road, lane, location)
        {
            Assert.NotNull(targetVehicle, "targetVehicle cannot be null");
            TargetVehicle = targetVehicle;
        }

        #region Properties
    
        /// <summary>
        /// The target vehicle this spike strip tries to target.
        /// </summary>
        private Vehicle TargetVehicle { get; }

        private bool IsVehicleInstanceInvalid => TargetVehicle == null || !TargetVehicle.IsValid();
            
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
            if (IsVehicleInstanceInvalid || State is ESpikeStripState.Hit or ESpikeStripState.Bypassed)
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

        #endregion
    }
}