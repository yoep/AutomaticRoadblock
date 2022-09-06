using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.SpikeStrip.Dispatcher;
using AutomaticRoadblocks.SpikeStrip.Slot;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Vehicles;
using JetBrains.Annotations;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    /// <summary>
    /// This abstract implementation of <see cref="IRoadblock"/> verifies certain states such
    /// as <see cref="ERoadblockState.Hit"/> or <see cref="ERoadblockState.Bypassed"/>.
    /// </summary>
    internal abstract class AbstractPursuitRoadblock : AbstractRoadblock
    {
        private const float BypassTolerance = 20f;

        private static readonly Random Random = new();

        private float _lastKnownDistanceToRoadblock = 9999f;

        protected AbstractPursuitRoadblock(ISpikeStripDispatcher spikeStripDispatcher, Road road, BarrierType mainBarrierType, Vehicle targetVehicle,
            bool limitSpeed, bool addLights,
            bool spikeStripEnabled)
            : base(road, mainBarrierType, targetVehicle != null ? targetVehicle.Heading : 0f, limitSpeed, addLights)
        {
            Assert.NotNull(spikeStripDispatcher, "spikeStripDispatcher cannot be null");
            Assert.NotNull(targetVehicle, "targetVehicle cannot be null");
            SpikeStripDispatcher = spikeStripDispatcher;
            TargetVehicle = targetVehicle;
            SpikeStripEnabled = spikeStripEnabled;

            Initialize();
        }

        #region Properties

        /// <summary>
        /// Get the target vehicle of this roadblock.
        /// </summary>
        [CanBeNull]
        protected Vehicle TargetVehicle { get; }

        /// <summary>
        /// The indication if a spike strip will be spawned along the road block.
        /// </summary>
        protected bool SpikeStripEnabled { get; }

        /// <summary>
        /// Verify if the <see cref="TargetVehicle"/> instance is invalidated by the game.
        /// This might be the case by the pursuit suddenly being (forcefully) ended.
        /// </summary>
        private bool IsVehicleInstanceInvalid => TargetVehicle == null || !TargetVehicle.IsValid();

        /// <summary>
        /// The spike strip dispatcher to use for deploying spike strip slots.
        /// </summary>
        private ISpikeStripDispatcher SpikeStripDispatcher { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool Spawn()
        {
            var result = base.Spawn();
            Monitor();

            return result;
        }

        public override string ToString()
        {
            return $"{nameof(SpikeStripEnabled)}: {SpikeStripEnabled}, {base.ToString()}";
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
            Road.Lane spikeStripLane = null;

            if (SpikeStripEnabled)
            {
                spikeStripLane = lanesToBlock[Random.Next(lanesToBlock.Count)];
                Logger.Trace($"Adding spike strip on lane {spikeStripLane}");
            }

            return lanesToBlock
                .Select(lane => lane == spikeStripLane
                    ? CreateSpikeStripSlot(lane, Heading, TargetVehicle, IsLightsEnabled)
                    : CreateSlot(lane, Heading, TargetVehicle, IsLightsEnabled))
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
                new InstanceSlot(EEntityType.CopVehicle, roadPosition, TargetHeading + 25,
                    (position, heading) => VehicleFactory.CreateWithModel(vehicleModel, position, heading)),
                new InstanceSlot(EEntityType.CopPed, roadPosition, TargetHeading,
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
                Instances.Add(new InstanceSlot(EEntityType.Scenery, startPosition, TargetHeading,
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
                while (State == ERoadblockState.Active)
                {
                    try
                    {
                        VerifyIfRoadblockIsBypassed();
                        VerifyIfRoadblockIsHit();
                        VerifyRoadblockCopKilled();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"An error occurred while monitoring the roadblock, {ex.Message}", ex);
                    }

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

            var currentDistance = TargetVehicle.DistanceTo(OffsetPosition);

            if (currentDistance < _lastKnownDistanceToRoadblock)
            {
                _lastKnownDistanceToRoadblock = currentDistance;
            }
            else if (Math.Abs(currentDistance - _lastKnownDistanceToRoadblock) > BypassTolerance)
            {
                BlipFlashNewState(Color.LightGray);
                Release();
                UpdateState(ERoadblockState.Bypassed);
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

            if (!TargetVehicle.HasBeenDamagedByAnyVehicle)
                return;

            Logger.Trace("Collision has been detected for target vehicle");
            if (Slots.Any(HasBeenDamagedBy))
            {
                Logger.Debug("Determined that the collision must have been against a roadblock slot");
                BlipFlashNewState(Color.Green);
                Release();
                UpdateState(ERoadblockState.Hit);
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
                .OfType<IPursuitRoadblockSlot>()
                .Any(x => x.HasCopBeenKilledByTarget);

            if (hasACopBeenKilled)
                InvokeRoadblockCopKilled();
        }

        private bool HasBeenDamagedBy(IRoadblockSlot slot)
        {
            if (slot.Vehicle != null)
                return slot.Vehicle.HasBeenDamagedBy(TargetVehicle);

            Logger.Warn($"Unable to verify the vehicle collision for slot {slot}, vehicle is null");
            return false;
        }

        private void BlipFlashNewState(Color color)
        {
            if (Blip == null)
                return;

            Blip.Color = color;
            Blip.Flash(500, BlipFlashDuration);
        }

        private void InvalidateTheRoadblock()
        {
            Logger.Warn("Unable to verify of the roadblock status (bypass/hit), the vehicle instance is no longer valid");
            // invalidate the roadblock so it can be cleaned up
            // this will also stop the monitor from running as we're not able
            // to determine any status anymore
            UpdateState(ERoadblockState.Invalid);
        }

        private IRoadblockSlot CreateSpikeStripSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool addLights)
        {
            return new SpikeStripSlot(SpikeStripDispatcher, Road, lane, targetVehicle, heading, addLights);
        }

        #endregion
    }
}