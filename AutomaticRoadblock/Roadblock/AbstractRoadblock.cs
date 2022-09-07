using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Roads;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Type;
using Rage;
using VehicleType = AutomaticRoadblocks.Vehicles.VehicleType;

namespace AutomaticRoadblocks.Roadblock
{
    /// <summary>
    /// The abstract basic implementation of <see cref="IRoadblock"/> which can be used for manual and pursuit roadblocks.
    /// This implementation does not verify any states, use <see cref="AbstractPursuitRoadblock"/> instead.
    /// <remarks>Make sure that the <see cref="Initialize"/> method is called within the constructor after all properties/fields are set for the roadblock.</remarks>
    /// </summary>
    public abstract class AbstractRoadblock : IRoadblock
    {
        protected const float SpeedLimit = 5f;
        protected const float LaneHeadingTolerance = 40f;
        protected const int BlipFlashDuration = 3500;
        protected const float AdditionalClippingSpace = 0.5f;

        protected static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        protected static readonly IGame Game = IoC.Instance.GetInstance<IGame>();
        
        protected readonly bool IsSpeedLimitEnabled;
        protected readonly bool IsLightsEnabled;

        protected Blip Blip;
        private int _speedZoneId;

        /// <summary>
        /// Initialize a new roadblock instance.
        /// </summary>
        /// <param name="road">The road of that the roadblock will block.</param>
        /// <param name="mainBarrierType">The main barrier used within the slots.</param>
        /// <param name="targetHeading">The target heading in which the roadblock should be placed.</param>
        /// <param name="limitSpeed">Indicates if a speed limit should be added.</param>
        /// <param name="addLights">Indicates if light props should be added.</param>
        /// <param name="offset">The offset placement in regards to the road node.</param>
        internal AbstractRoadblock(Road road, BarrierType mainBarrierType, float targetHeading, bool limitSpeed, bool addLights, float offset = 0f)
        {
            Assert.NotNull(road, "road cannot be null");
            Assert.NotNull(mainBarrierType, "mainBarrierType cannot be null");
            Road = road;
            MainBarrierType = mainBarrierType;
            TargetHeading = targetHeading;
            Offset = offset;
            IsSpeedLimitEnabled = limitSpeed;
            IsLightsEnabled = addLights;
        }

        #region Properties

        /// <inheritdoc />
        public uint LastStateChange { get; private set; }

        /// <summary>
        /// Get the level of the roadblock.
        /// </summary>
        public abstract RoadblockLevel Level { get; }

        /// <inheritdoc />
        public ERoadblockState State { get; private set; } = ERoadblockState.Preparing;

        /// <inheritdoc />
        public Vector3 Position => Road.Position;
        
        /// <summary>
        /// The offset position for the roadblock.
        /// </summary>
        public Vector3 OffsetPosition => Road.Position + MathHelper.ConvertHeadingToDirection(Road.Node.Heading) * Offset;

        /// <inheritdoc />
        public float Heading { get; private set; }

        /// <inheritdoc />
        public Road Road { get; }

        /// <summary>
        /// Get the main barrier type of the roadblock.
        /// </summary>
        protected BarrierType MainBarrierType { get; }

        /// <summary>
        /// Get the target heading of the roadblock.
        /// </summary>
        protected float TargetHeading { get; }

        /// <summary>
        /// The placement offset in regards to the node.
        /// </summary>
        public float Offset { get; }

        /// <summary>
        /// Get the generated slots for this roadblock.
        /// </summary>
        protected IReadOnlyList<IRoadblockSlot> Slots { get; private set; }

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

        /// <inheritdoc />
        public event RoadblockEvents.RoadblockCopsJoiningPursuit RoadblockCopsJoiningPursuit;

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => Instances.Any(x => x.IsPreviewActive) ||
                                       Slots.Any(x => x.IsPreviewActive);

        /// <inheritdoc />
        public void CreatePreview()
        {
            Logger.Trace($"Creating roadblock preview for {this}");
            CreateBlip();
            Logger.Debug($"Creating a total of {Slots.Count} slot previews for the roadblock preview");
            foreach (var roadblockSlot in Slots)
            {
                roadblockSlot.CreatePreview();
            }

            Logger.Trace($"Creating roadblock road preview");
            Road.CreatePreview();
            Instances.ForEach(x => x.CreatePreview());
            DoInternalDebugPreviewCreation();
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            DeleteBlip();
            Slots.ToList().ForEach(x => x.DeletePreview());
            Road.DeletePreview();
            Instances.ForEach(x => x.DeletePreview());
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            UpdateState(ERoadblockState.Disposing);

            if (IsPreviewActive)
                DeletePreview();

            DeleteBlip();
            DeleteSpeedZone();
            Slots.ToList().ForEach(x => x.Dispose());
            Instances.ForEach(x => x.Dispose());

            UpdateState(ERoadblockState.Disposed);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Spawn the roadblock in the world.
        /// </summary>
        public virtual bool Spawn()
        {
            var result = false;
            
            try
            {
                Logger.Trace("Spawning roadblock");

                Slots.ToList().ForEach(x => x.Spawn());
                result = Instances.All(x => x.Spawn());
                UpdateState(ERoadblockState.Active);

                CreateBlip();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to spawn roadblock", ex);
                UpdateState(ERoadblockState.Error);
            }

            return result;
        }

        /// <inheritdoc />
        public void Release()
        {
            // verify if the roadblock is still active
            // otherwise, we cannot release the entities
            if (State != ERoadblockState.Active)
                return;

            InvokeCopsJoiningPursuit();
            ReleaseEntitiesToLspdfr();
            UpdateState(ERoadblockState.Released);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Level)}: {Level}, {nameof(State)}: {State}, Number of {nameof(Slots)}: [{Slots.Count}]\n" +
                   $"--- {nameof(Slots)} ---\n" +
                   $"{string.Join("\n", Slots)}\n" +
                   $"--- {nameof(Road)} ---\n" +
                   $"{Road}";
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
        /// Initialize additional vehicles for this roadblock.
        /// Additional vehicles can exists out of emergency service such as EMS, etc.
        /// </summary>
        protected virtual void InitializeAdditionalVehicles()
        {
            // no-op
        }

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
        protected void UpdateState(ERoadblockState state)
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
        /// <returns>Returns a list of created slots.</returns>
        protected abstract IReadOnlyList<IRoadblockSlot> CreateRoadblockSlots(IReadOnlyList<Road.Lane> lanesToBlock);

        /// <summary>
        /// Initialize the roadblock data.
        /// This method will calculate the slots and create all necessary entities for the roadblock.
        /// </summary>
        protected void Initialize()
        {
            InitializeRoadblockSlots();
            InitializeAdditionalVehicles();
            InitializeScenery();
            InitializeSpeedLimit(IsSpeedLimitEnabled);

            if (IsLightsEnabled)
                InitializeLights();
        }

        /// <summary>
        /// Indicate that a cop from the given roadblock slot was killed.
        /// </summary>
        protected void InvokeRoadblockCopKilled()
        {
            RoadblockCopKilled?.Invoke(this);
        }

        /// <summary>
        /// Indicate that the cops of this roadblock can join the pursuit.
        /// </summary>
        protected void InvokeCopsJoiningPursuit()
        {
            RoadblockCopsJoiningPursuit?.Invoke(this, RetrieveCopsJoiningThePursuit());
        }

        /// <summary>
        /// Retrieve a list of cops who will join the pursuit.
        /// </summary>
        /// <returns>Returns the cops joining the pursuit.</returns>
        protected virtual IEnumerable<Ped> RetrieveCopsJoiningThePursuit()
        {
            var copsJoining = new List<Ped>();

            foreach (var slot in Slots)
            {
                copsJoining.AddRange(slot.Cops.Select(x => x.GameInstance));
            }

            return copsJoining;
        }

        private void InitializeRoadblockSlots()
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
            Slots = CreateRoadblockSlots(lanesToBlock);
            PreventSlotVehiclesClipping();
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
                _speedZoneId = RoadUtils.CreateSpeedZone(OffsetPosition, 10f, SpeedLimit);
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

            Logger.Trace($"Creating roadblock blip at {OffsetPosition}");
            Blip = new Blip(OffsetPosition)
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
                        result = x.Position.DistanceTo(lastLane.Position) >= 4f;

                    lastLane = x;
                    return result;
                })
                .ToList();
            Logger.Debug($"Filtered a total of {lanesToBlock.Count - filteredLanesToBlock.Count} lanes which are too close to each other");

            if (filteredLanesToBlock.Count != 0)
                return filteredLanesToBlock;

            Logger.Warn("Lanes too close filter has filtered out all lanes, resetting to original list");
            return lanesToBlock.ToList();
        }

        private void PreventSlotVehiclesClipping()
        {
            for (var i = 0; i < Slots.Count - 1; i++)
            {
                var currentSlot = Slots[i];
                var nextSlot = Slots[i + 1];

                // if the current slot doesn't contain any vehicle
                // skip the clipping calculation and move to the next one
                if (currentSlot.VehicleType == VehicleType.None)
                    continue;

                var currentSlotDifference = CalculateSlotVehicleDifference(currentSlot);
                var nextSlotDifference = CalculateSlotVehicleDifference(nextSlot);

                // verify if the slot difference is smaller than 0
                // this means that the vehicle is exceeding the lane width and might clip into the other vehicle
                if (currentSlotDifference > 0)
                    continue;

                Logger.Trace($"Current slot vehicle is exceeding by {currentSlotDifference}");
                // check if there is enough space between this lane and the other one
                // if so, we're using the next lane space for the current exceeding slot vehicle
                if (nextSlotDifference > 0 && nextSlotDifference - Math.Abs(currentSlotDifference) >= 0)
                {
                    Logger.Trace($"Next slot had enough space ({nextSlotDifference}) for the current exceeding slot vehicle");
                    continue;
                }

                // move the current slot vehicle position by the difference
                var newPosition = currentSlot.OffsetPosition + MathHelper.ConvertHeadingToDirection(Heading - 90) *
                    (Math.Abs(currentSlotDifference) + AdditionalClippingSpace);
                Logger.Debug(
                    $"Slot vehicle is clipping into next slot by ({currentSlotDifference}), old position {currentSlot.OffsetPosition}, new position {newPosition}");
                currentSlot.ModifyVehiclePosition(newPosition);
            }
        }

        private float CalculateSlotVehicleDifference(IRoadblockSlot slot)
        {
            var laneWidth = slot.Lane.Width;
            var vehicleLength = slot.VehicleModel.Dimensions.Y;

            return laneWidth - vehicleLength;
        }

        private void ReleaseEntitiesToLspdfr()
        {
            Logger.Debug("Releasing cop peds to LSPDFR");
            foreach (var slot in Slots)
            {
                slot.Release();
            }

            Game.NewSafeFiber(() =>
            {
                GameFiber.Wait(BlipFlashDuration);
                DeleteBlip();
            }, "Roadblock.ReleaseEntitiesToLspdfr");
        }

        [Conditional("DEBUG")]
        private void DoInternalDebugPreviewCreation()
        {
            Game.NewSafeFiber(() =>
            {
                while (IsPreviewActive)
                {
                    foreach (var ped in RetrieveCopsJoiningThePursuit())
                    {
                        GameUtils.CreateMarker(ped.Position, EMarkerType.MarkerTypeUpsideDownCone, Color.Lime, 1f, 1f, false);
                    }

                    Game.FiberYield();
                }
            }, "Roadblock.Preview");
        }

        #endregion
    }
}