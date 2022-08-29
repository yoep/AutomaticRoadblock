using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Vehicles;
using JetBrains.Annotations;
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
        [CanBeNull]
        protected Vehicle Vehicle { get; }

        /// <summary>
        /// Verify if the <see cref="Vehicle"/> instance is invalidated by the game.
        /// This might be the case by the pursuit suddenly being (forcefully) ended.
        /// </summary>
        private bool IsVehicleInstanceInvalid => Vehicle == null || !Vehicle.IsValid();

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

        /// <summary>
        /// Create a chase vehicle for this roadblock.
        /// The chase vehicle will be created on the right side of the road.
        /// </summary>
        protected void CreateChaseVehicle(Model vehicleModel)
        {
            Assert.NotNull(vehicleModel, "vehicleModel cannot be null");
            var roadPosition = Road.RightSide + ChaseVehiclePositionDirection();

            Instances.AddRange(new[]
            {
                new InstanceSlot(EntityType.CopVehicle, roadPosition, TargetHeading + 25,
                    (position, heading) => VehicleFactory.CreateWithModel(vehicleModel, position, heading)),
                new InstanceSlot(EntityType.CopPed, roadPosition, TargetHeading,
                    (position, heading) =>
                        PedFactory.CreateCopWeaponsForModel(PedFactory.CreateCopForVehicle(vehicleModel, position, heading)))
            });

            // create buffer barrels behind the vehicle
            CreateChaseVehicleBufferBarrels(roadPosition);
        }

        private void CreateChaseVehicleBufferBarrels(Vector3 chasePosition)
        {
            var rowPosition = chasePosition + MathHelper.ConvertHeadingToDirection(TargetHeading - 180) * 4f;
            var nextPositionDistance = BarrierType.BarrelTrafficCatcher.Width + BarrierType.BarrelTrafficCatcher.Spacing;
            var nextPositionDirection = MathHelper.ConvertHeadingToDirection(TargetHeading - 90);
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(TargetHeading + 90) * (nextPositionDistance * 2f);

            for (var i = 0; i < 5; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.Scenery, startPosition, TargetHeading,
                    (position, heading) => BarrierFactory.Create(BarrierType.BarrelTrafficCatcher, position, heading)));
                startPosition += nextPositionDirection * nextPositionDistance;
            }
        }

        private Vector3 ChaseVehiclePositionDirection()
        {
            return MathHelper.ConvertHeadingToDirection(TargetHeading) * 15f +
                   MathHelper.ConvertHeadingToDirection(TargetHeading - 90) * 1.5f;
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
            if (IsVehicleInstanceInvalid)
            {
                InvalidateTheRoadblock();
                return;
            }

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
            if (IsVehicleInstanceInvalid)
            {
                InvalidateTheRoadblock();
                return;
            }

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
                InvokeRoadblockCopKilled();
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

        private void InvalidateTheRoadblock()
        {
            Logger.Warn("Unable to verify of the roadblock status (bypass/hit), the vehicle instance is no longer valid");
            // invalidate the roadblock so it can be cleaned up
            // this will also stop the monitor from running as we're not able
            // to determine any status anymore
            UpdateState(RoadblockState.Invalid);
        }

        #endregion
    }
}