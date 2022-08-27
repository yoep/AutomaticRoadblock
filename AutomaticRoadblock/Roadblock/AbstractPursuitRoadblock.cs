using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    /// <summary>
    /// This abstract implementation of <see cref="IRoadblock"/> verifies certain states such
    /// as <see cref="RoadblockState.Hit"/> or <see cref="RoadblockState.Bypassed"/>.
    /// </summary>
    internal abstract class AbstractPursuitRoadblock : AbstractRoadblock
    {
        protected const float BypassTolerance = 20f;

        private float _lastKnownDistanceToRoadblock = 9999f;

        protected AbstractPursuitRoadblock(Road road, BarrierType mainBarrierType, Vehicle vehicle, bool limitSpeed, bool addLights)
            : base(road, mainBarrierType, vehicle != null ? vehicle.Heading : 0f, limitSpeed, addLights)
        {
            Assert.NotNull(vehicle, "vehicle cannot be null");
            Vehicle = vehicle;

            Initialize();
        }

        #region Properties

        /// <summary>
        /// Get the target vehicle of this roadblock.
        /// </summary>
        protected Vehicle Vehicle { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Spawn()
        {
            base.Spawn();
            Monitor();
        }

        #endregion

        #region Functions

        /// <summary>
        /// Create a slot for this roadblock for the given lane.
        /// </summary>
        /// <param name="lane">The lane for the slot to block.</param>
        /// <param name="heading">The heading of the block.</param>
        /// <param name="targetVehicle">The target vehicle.</param>
        /// <param name="shouldAddLights">Set if light scenery should be added.</param>
        /// <returns>Returns the created slot for the roadblock.</returns>
        protected abstract IRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights);

        /// <inheritdoc />
        protected override IReadOnlyList<IRoadblockSlot> CreateRoadblockSlots(IReadOnlyList<Road.Lane> lanesToBlock)
        {
            return lanesToBlock
                .Select(lane => CreateSlot(lane, Heading, Vehicle, IsLightsEnabled))
                .ToList();
        }

        private void Monitor()
        {
            Game.NewSafeFiber(() =>
            {
                while (State == RoadblockState.Active)
                {
                    VerifyIfRoadblockIsBypassed();
                    VerifyIfRoadblockIsHit();
                    VerifyRoadblockCopKilled();
                    Game.FiberYield();
                }
            }, "PursuitRoadblock.Monitor");
        }

        private void VerifyIfRoadblockIsBypassed()
        {
            var currentDistance = Vehicle.DistanceTo(Position);

            if (currentDistance < _lastKnownDistanceToRoadblock)
            {
                _lastKnownDistanceToRoadblock = currentDistance;
            }
            else if (Math.Abs(currentDistance - _lastKnownDistanceToRoadblock) > BypassTolerance)
            {
                BlipFlashNewState(Color.LightGray);
                Release();
                UpdateState(RoadblockState.Bypassed);
                Logger.Info("Roadblock has been bypassed");
            }
        }

        private void VerifyIfRoadblockIsHit()
        {
            if (!Vehicle.HasBeenDamagedByAnyVehicle)
                return;

            Logger.Trace("Collision has been detected for target vehicle");
            if (Slots.Any(HasBeenDamagedBy))
            {
                Logger.Debug("Determined that the collision must have been against a roadblock slot");
                BlipFlashNewState(Color.Green);
                Release();
                UpdateState(RoadblockState.Hit);
                Logger.Info("Roadblock has been hit by the suspect");
            }
            else
            {
                Logger.Debug("Determined that the collision was not the roadblock");
            }
        }

        private void VerifyRoadblockCopKilled()
        {
            var hasACopBeenKilled = Slots
                .Select(x => (IPursuitRoadblockSlot)x)
                .Any(x => x.HasCopBeenKilledByTarget);

            if (hasACopBeenKilled)
                InvokedRoadblockCopKilled();
        }

        private bool HasBeenDamagedBy(IRoadblockSlot slot)
        {
            if (slot.Vehicle != null)
                return slot.Vehicle.HasBeenDamagedBy(Vehicle);

            Logger.Warn($"Unable to verify the vehicle collision for slot {slot}, vehicle is null");
            return false;
        }

        private void BlipFlashNewState(Color color)
        {
            Blip.Color = color;
            Blip.Flash(500, BlipFlashDuration);
        }

        #endregion
    }
}