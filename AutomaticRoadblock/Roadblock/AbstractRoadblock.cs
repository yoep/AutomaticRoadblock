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
    /// <summary>
    /// The abstract basic implementation of <see cref="IRoadblock"/> which can be used for manual and pursuit roadblocks.
    /// This implementation does not verify any states, use <see cref="AbstractPursuitRoadblock"/> instead.
    /// </summary>
    public abstract class AbstractRoadblock : IRoadblock
    {
        protected const float SpeedLimit = 5f;
        protected const float LaneHeadingTolerance = 40f;
        protected const int BlipFlashDuration = 3000;

        protected readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        protected readonly IGame Game = IoC.Instance.GetInstance<IGame>();

        protected Blip Blip;
        private int _speedZoneId;

        /// <summary>
        /// Initialize a new roadblock instance.
        /// </summary>
        /// <param name="road">The road of that the roadblock will block.</param>
        /// <param name="mainBarrierType">The main barrier used within the slots.</param>
        /// <param name="vehicle">The vehicle which is targeted by the roadblock (optional).</param>
        /// <param name="targetHeading">The target heading in which the roadblock should be placed.</param>
        /// <param name="limitSpeed">Indicates if a speed limit should be added.</param>
        /// <param name="addLights">Indicates if light props should be added.</param>
        internal AbstractRoadblock(Road road, BarrierType mainBarrierType, Vehicle vehicle, float targetHeading, bool limitSpeed, bool addLights)
        {
            Assert.NotNull(road, "road cannot be null");
            Assert.NotNull(mainBarrierType, "mainBarrierType cannot be null");
            Road = road;
            MainBarrierType = mainBarrierType;
            Vehicle = vehicle;
            TargetHeading = targetHeading;

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
        /// Get the main barrier type of the roadblock.
        /// </summary>
        protected BarrierType MainBarrierType { get; }

        /// <summary>
        /// Get the target vehicle of this roadblock.
        /// </summary>
        protected Vehicle Vehicle { get; }

        /// <summary>
        /// Get the target heading of the roadblock.
        /// </summary>
        protected float TargetHeading { get; }

        /// <summary>
        /// Get the generated slots for this roadblock.
        /// </summary>
        protected IReadOnlyCollection<IRoadblockSlot> Slots { get; private set; }

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
            .FirstOrDefault() || Slots
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
            Slots.ToList().ForEach(x => x.Dispose());
            Instances.ForEach(x => x.Dispose());

            UpdateState(RoadblockState.Disposed);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Spawn the roadblock in the world.
        /// </summary>
        public virtual void Spawn()
        {
            try
            {
                Logger.Trace("Spawning roadblock");
                UpdateState(RoadblockState.Active);

                Slots.ToList().ForEach(x => x.Spawn());
                Instances.ForEach(x => x.Spawn());

                CreateBlip();
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
        /// Retrieve the lanes of the road which should be blocked.
        /// </summary>
        /// <returns>Returns the lanes to block.</returns>
        protected virtual IReadOnlyList<Road.Lane> LanesToBlock()
        {
            // filter any lanes which are to close to each other
            return FilterLanesWhichAreTooCloseToEachOther(Road.Lanes);
        }

        /// <summary>
        /// Delete the blip of the roadblock.
        /// </summary>
        protected void DeleteBlip()
        {
            if (Blip == null)
                return;

            Blip.Delete();
            Blip = null;
        }

        /// <summary>
        /// Update the state of the roadblock.
        /// </summary>
        /// <param name="state">The new state of the roadblock.</param>
        protected void UpdateState(RoadblockState state)
        {
            if (State == state)
                return;

            LastStateChange = Game.GameTime;
            State = state;
            RoadblockStateChanged?.Invoke(this, state);
        }

        /// <summary>
        /// Create roadblock slots for the given lanes.
        /// </summary>
        /// <param name="lanesToBlock">The lanes to block.</param>
        /// <param name="addLights">Indicates if lights should be added.</param>
        /// <returns>Returns a list of created slots.</returns>
        protected abstract IReadOnlyCollection<IRoadblockSlot> CreateRoadblockSlots(IReadOnlyList<Road.Lane> lanesToBlock, bool addLights);

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
                .Where(x => Math.Abs(x - TargetHeading) < LaneHeadingTolerance)
                .DefaultIfEmpty(Road.Lanes[0].Heading)
                .First();
            var lanesToBlock = LanesToBlock();

            if (lanesToBlock.Count == 0)
            {
                Logger.Warn("Lanes to block returned 0 lanes, resetting and using all road lanes instead");
                lanesToBlock = Road.Lanes;
            }

            Logger.Trace($"Roadblock will block {lanesToBlock.Count} lanes");
            Slots = CreateRoadblockSlots(lanesToBlock, addLights);
        }

        /// <summary>
        /// Indicate that a cop from the given roadblock slot was killed.
        /// </summary>
        /// <param name="roadblockSlot">The slot from which the cop was killed.</param>
        protected void RoadblockSlotCopKilled(IRoadblockSlot roadblockSlot)
        {
            RoadblockCopKilled?.Invoke(this);
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
            if (Blip != null)
                return;

            Blip = new Blip(Postion)
            {
                IsRouteEnabled = false,
                IsFriendly = true,
                Scale = 1f,
                Color = Color.LightBlue
            };
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