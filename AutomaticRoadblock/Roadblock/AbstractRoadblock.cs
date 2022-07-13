using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    public abstract class AbstractRoadblock : IRoadblock
    {
        protected const float LaneHeadingTolerance = 40f;
        protected const float BypassTolerance = 20f;
        protected const float SpeedLimit = 5f;
        protected const int BlipFlashDuration = 3000;

        protected readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        protected readonly IGame Game = IoC.Instance.GetInstance<IGame>();

        private Blip _blip;
        private int _speedZoneId;
        private float _lastKnownDistanceToRoadblock = 9999f;

        internal AbstractRoadblock(Road road, Vehicle vehicle, bool limitSpeed, bool addLights)
        {
            Assert.NotNull(road, "road cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");
            Road = road;
            Vehicle = vehicle;

            Init(limitSpeed, addLights);
        }

        #region Properties

        /// <inheritdoc />
        public uint LastStateChange { get; private set; }

        /// <summary>
        /// Get the level of the roadblock.
        /// </summary>
        public abstract RoadblockLevel Level { get; }

        /// <inheritdoc />
        public RoadblockState State { get; private set; } = RoadblockState.Preparing;

        /// <summary>
        /// Get the central position of the roadblock.
        /// </summary>
        public Vector3 Postion => Road.Position;

        /// <summary>
        /// Get the heading of the roadblock.
        /// </summary>
        public float Heading { get; private set; }

        /// <summary>
        /// Get the road of this roadblock.
        /// </summary>
        protected Road Road { get; }

        /// <summary>
        /// Get the target vehicle of this roadblock.
        /// </summary>
        protected Vehicle Vehicle { get; }

        /// <summary>
        /// Get the generated slots for this roadblock.
        /// </summary>
        protected List<IRoadblockSlot> Slots { get; } = new();

        /// <summary>
        /// Get the scenery slots for this roadblock.
        /// </summary>
        protected List<InstanceSlot> Instances { get; } = new();

        #endregion

        #region Events

        /// <inheritdoc />
        public event RoadblockEvents.RoadblockStateChanged RoadblockStateChanged;

        /// <inheritdoc />
        public event RoadblockEvents.RoadblockCopKilled RoadblockCopKilled;

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => Instances
            .Select(x => x.IsPreviewActive)
            .FirstOrDefault();

        /// <inheritdoc />
        public void CreatePreview()
        {
            CreateBlip();
            foreach (var roadblockSlot in Slots)
            {
                roadblockSlot.CreatePreview();
            }

            Road.CreatePreview();
            Instances.ForEach(x => x.CreatePreview());
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            DeleteBlip();
            foreach (var roadblockSlot in Slots)
            {
                roadblockSlot.DeletePreview();
            }

            Road.DeletePreview();
            Instances.ForEach(x => x.DeletePreview());
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            UpdateState(RoadblockState.Disposing);

            if (IsPreviewActive)
                DeletePreview();

            DeleteBlip();
            DeleteSpeedZone();
            Slots.ForEach(x => x.Dispose());
            Instances.ForEach(x => x.Dispose());

            UpdateState(RoadblockState.Disposed);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Spawn the roadblock in the world.
        /// </summary>
        public void Spawn()
        {
            try
            {
                Logger.Trace("Spawning roadblock");
                UpdateState(RoadblockState.Active);

                Slots.ForEach(x => x.Spawn());
                Instances.ForEach(x => x.Spawn());

                CreateBlip();
                Monitor();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to spawn roadblock", ex);
                UpdateState(RoadblockState.Error);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Level)}: {Level}\n" +
                   $"{nameof(State)}: {State}\n" +
                   $"{nameof(Slots)}: [{Slots.Count}]{string.Join(",", Slots)}\n" +
                   $"{nameof(Road)}: {Road}";
        }

        #endregion

        #region Functions

        /// <summary>
        /// Initialize the scenery slots of this roadblock.
        /// </summary>
        protected abstract void InitializeScenery();

        /// <summary>
        /// Initialize the light slots of this roadblock.
        /// </summary>
        protected abstract void InitializeLights();

        /// <summary>
        /// Create a slot for this roadblock for the given lane.
        /// </summary>
        /// <param name="lane">The lane for the slot to block.</param>
        /// <param name="heading">The heading of the block.</param>
        /// <param name="targetVehicle">The target vehicle.</param>
        /// <param name="shouldAddLights">Set if light scenery should be added.</param>
        /// <returns>Returns the created slot for the roadblock.</returns>
        protected abstract IRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights);

        /// <summary>
        /// Retrieve the lanes of the road which should be blocked.
        /// </summary>
        /// <returns>Returns the lanes to block.</returns>
        protected virtual IReadOnlyList<Road.Lane> LanesToBlock()
        {
            // filter any lanes which are to close to each other
            return FilterLanesWhichAreTooCloseToEachOther(Road.Lanes);
        }

        private void Init(bool limitSpeed, bool shouldAddLights)
        {
            InitializeRoadblockSlots(shouldAddLights);
            InitializeScenery();
            InitializeSpeedLimit(limitSpeed);

            if (shouldAddLights)
                InitializeLights();
        }

        private void InitializeRoadblockSlots(bool addLights)
        {
            Heading = Road.Lanes
                .Select(x => x.Heading)
                .Where(x => Math.Abs(x - Vehicle.Heading) < LaneHeadingTolerance)
                .DefaultIfEmpty(Road.Lanes[0].Heading)
                .First();
            var lanesToBlock = LanesToBlock();

            Logger.Trace($"Roadblock will block {lanesToBlock.Count} lanes");
            foreach (var lane in lanesToBlock)
            {
                var roadblockSlot = CreateSlot(lane, Heading, Vehicle, addLights);
                roadblockSlot.RoadblockCopKilled += RoadblockSlotCopKilled;
                Slots.Add(roadblockSlot);
            }
        }

        private void InitializeSpeedLimit(bool limitSpeed)
        {
            if (!limitSpeed)
                return;

            Logger.Trace("Creating speed zone at roadblock");
            CreateSpeedZoneLimit();
        }

        private void CreateSpeedZoneLimit()
        {
            try
            {
                _speedZoneId = RoadUtils.CreateSpeedZone(Postion, 10f, SpeedLimit);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create roadblock speed zone, {ex.Message}", ex);
            }
        }

        private void DeleteSpeedZone()
        {
            if (_speedZoneId != 0)
                RoadUtils.RemoveSpeedZone(_speedZoneId);
        }

        private void CreateBlip()
        {
            if (_blip != null)
                return;

            _blip = new Blip(Postion)
            {
                IsRouteEnabled = false,
                IsFriendly = true,
                Scale = 1f,
                Color = Color.LightBlue
            };
        }

        private void DeleteBlip()
        {
            if (_blip == null)
                return;

            _blip.Delete();
            _blip = null;
        }

        private void Monitor()
        {
            Game.NewSafeFiber(() =>
            {
                while (State == RoadblockState.Active)
                {
                    VerifyIfRoadblockIsBypassed();
                    VerifyIfRoadblockIsHit();
                    Game.FiberYield();
                }
            }, "Roadblock.Monitor");
        }

        private void ReleaseEntitiesToLspdfr()
        {
            Logger.Debug("Releasing cop peds to LSPDFR");
            foreach (var slot in Slots)
            {
                slot.ReleaseToLspdfr();
            }

            IoC.Instance.GetInstance<IGame>().NewSafeFiber(() =>
            {
                GameFiber.Wait(BlipFlashDuration);
                DeleteBlip();
            }, "Roadblock.ReleaseEntitiesToLspdfr");
        }

        private void VerifyIfRoadblockIsBypassed()
        {
            var currentDistance = Vehicle.DistanceTo(Postion);

            if (currentDistance < _lastKnownDistanceToRoadblock)
            {
                _lastKnownDistanceToRoadblock = currentDistance;
            }
            else if (Math.Abs(currentDistance - _lastKnownDistanceToRoadblock) > BypassTolerance)
            {
                BlipFlashNewState(Color.LightGray);
                UpdateState(RoadblockState.Bypassed);
                ReleaseEntitiesToLspdfr();
                Logger.Info("Roadblock has been bypassed");
            }
        }

        private void VerifyIfRoadblockIsHit()
        {
            if (!Vehicle.HasBeenDamagedByAnyVehicle)
                return;

            Logger.Trace("Collision has been detected for target vehicle");
            if (Slots.Any(slot => slot.Vehicle.HasBeenDamagedBy(Vehicle)))
            {
                Logger.Debug("Determined that the collision must have been against a roadblock slot");
                BlipFlashNewState(Color.Green);
                UpdateState(RoadblockState.Hit);
                ReleaseEntitiesToLspdfr();
                Logger.Info("Roadblock has been hit by the suspect");
            }
            else
            {
                Logger.Debug("Determined that the collision was not the roadblock");
            }
        }

        private void UpdateState(RoadblockState state)
        {
            if (State == state)
                return;

            LastStateChange = Game.GameTime;
            State = state;
            RoadblockStateChanged?.Invoke(this, state);
        }

        private void BlipFlashNewState(Color color)
        {
            _blip.Color = color;
            _blip.Flash(500, BlipFlashDuration);
        }

        private void RoadblockSlotCopKilled(IRoadblockSlot roadblockSlot)
        {
            RoadblockCopKilled?.Invoke(this);
        }

        private IReadOnlyList<Road.Lane> FilterLanesWhichAreTooCloseToEachOther(IReadOnlyList<Road.Lane> lanesToBlock)
        {
            Road.Lane lastLane = null;

            Logger.Trace("Filtering lanes which are too close to each-other");
            var filteredLanesToBlock = lanesToBlock.Where(x =>
                {
                    var result = true;

                    if (lastLane != null)
                        result = x.Position.DistanceTo(lastLane.Position) >= 5f;

                    lastLane = x;
                    return result;
                })
                .ToList();

            if (filteredLanesToBlock.Count != 0)
                return filteredLanesToBlock;

            Logger.Warn("Lanes too close filter has filtered out all lanes, resetting to original list");
            return lanesToBlock.ToList();
        }

        #endregion
    }
}