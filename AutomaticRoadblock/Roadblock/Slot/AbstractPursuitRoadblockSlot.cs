using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public abstract class AbstractPursuitRoadblockSlot : AbstractRoadblockSlot, IPursuitRoadblockSlot
    {
        private bool _monitorActive;

        protected AbstractPursuitRoadblockSlot(Road.Lane lane, BarrierModel mainBarrier, BarrierModel secondaryBarrier, EBackupUnit backupType, float heading,
            Vehicle targetVehicle, List<LightModel> lightSources, bool shouldAddLights, float offset = 0f, bool initialize = true)
            : base(lane, mainBarrier, secondaryBarrier, backupType, heading, shouldAddLights, true, offset)
        {
            Assert.NotNull(targetVehicle, "targetVehicle cannot be null");
            Assert.NotNull(lightSources, "lightSources cannot be null");
            TargetVehicle = targetVehicle;
            LightSources = lightSources;

            if (initialize)
                Initialize();

            StartHitDetection();
        }

        #region Properties

        /// <inheritdoc />
        public virtual IList<ARPed> CopsJoiningThePursuit => Cops;

        /// <inheritdoc />
        public event RoadblockEvents.RoadblockSlotHit RoadblockSlotHit;

        /// <summary>
        /// Get the target vehicle of the slot.
        /// </summary>
        protected Vehicle TargetVehicle { get; }

        /// <summary>
        /// The light sources of this slot.
        /// </summary>
        protected List<LightModel> LightSources { get; }

        /// <summary>
        /// Verify if the target or vehicle from this slot have been invalidated.
        /// </summary>
        private bool IsInvalidated => TargetVehicle == null || !TargetVehicle.IsValid() ||
                                      Vehicle == null || Vehicle.IsInvalid;

        #endregion

        #region IPursuitRoadblockSlot

        /// <inheritdoc />
        public bool HasCopBeenKilledByTarget => Instances.Where(x => x.Type == EEntityType.CopPed)
            .Select(x => (ARPed)x)
            .Where(x => x is { IsInvalid: false })
            .Select(x => x.GameInstance)
            .Where(x => x.IsDead)
            .Where(HasCopReceivedDamageFromVehicleOrSuspects)
            .Any(HasCopBeenKilledBySuspects);

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Release(bool releaseAll = false)
        {
            _monitorActive = false;
            DoInternalRelease(releaseAll ? Cops : CopsJoiningThePursuit);
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            _monitorActive = false;
            base.Dispose();
        }

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override void InitializeLights()
        {
            Instances.AddRange(LightSources
                .SelectMany(x => LightSourceFactory.Create(x, this)));
        }

        /// <summary>
        /// Invoke the <see cref="RoadblockEvents.RoadblockSlotHit"/> event.
        /// This will stop the internal monitor to detect for any further hit detections.
        /// </summary>
        /// <param name="type">The entity type that was hit</param>
        protected void InvokedRoadblockHitEvent(ERoadblockHitType type)
        {
            Logger.Debug($"Target vehicle has hit on {type} slot {this}");
            RoadblockSlotHit?.Invoke(this, type);
            _monitorActive = false;
        }

        private bool HasCopBeenKilledBySuspects(Entity cop)
        {
            if (TargetVehicle == null || !TargetVehicle.IsValid())
                return false;

            return cop.HasBeenDamagedBy(TargetVehicle) || TargetVehicle.Occupants.Any(cop.HasBeenDamagedBy);
        }

        private void StartHitDetection()
        {
            _monitorActive = true;
            GameUtils.NewSafeFiber(() =>
            {
                while (_monitorActive)
                {
                    VerifyHitByTargetVehicle();
                    GameFiber.Yield();
                }
            }, $"{GetType()}.HitDetection");
        }

        private void VerifyHitByTargetVehicle()
        {
            if (IsInvalidated)
            {
                _monitorActive = false;
                return;
            }

            if (!TargetVehicle.HasBeenDamagedByAnyVehicle && Cops.All(x => !x.GameInstance.HasBeenDamagedByAnyVehicle))
                return;

            // verify if any vehicle or cop have been hit by the target vehicle
            if (Vehicle.GameInstance.HasBeenDamagedBy(TargetVehicle))
            {
                InvokedRoadblockHitEvent(ERoadblockHitType.Vehicle);
            }
            else if (Cops.Any(x => x.GameInstance.HasBeenDamagedBy(TargetVehicle)))
            {
                InvokedRoadblockHitEvent(ERoadblockHitType.Cop);
            }
        }

        private static bool HasCopReceivedDamageFromVehicleOrSuspects(Entity x)
        {
            return x.HasBeenDamagedByAnyVehicle || x.HasBeenDamagedByAnyPed;
        }

        #endregion
    }
}