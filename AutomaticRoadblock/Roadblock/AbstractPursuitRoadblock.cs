using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Data;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Models;
using AutomaticRoadblocks.Roadblock.Data;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.SpikeStrip.Dispatcher;
using AutomaticRoadblocks.SpikeStrip.Slot;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Type;
using JetBrains.Annotations;
using LSPD_First_Response.Engine.Scripting.Entities;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    /// <summary>
    /// This abstract implementation of <see cref="IRoadblock"/> verifies certain states such
    /// as <see cref="ERoadblockState.Hit"/> or <see cref="ERoadblockState.Bypassed"/>.
    /// </summary>
    internal abstract class AbstractPursuitRoadblock : AbstractRoadblock<IPursuitRoadblockSlot>, IPursuitRoadblock
    {
        private const float BypassTolerance = 20f;
        private const string AudioRoadblockDeployed = "ROADBLOCK_DEPLOYED";
        private const string AudioRoadblockBypassed = "ROADBLOCK_BYPASSED";
        private const string AudioRoadblockHit = "ROADBLOCK_HIT";
        private const string AudioSpikeStripBypassed = "ROADBLOCK_SPIKESTRIP_BYPASSED";
        private const string AudioSpikeStripHit = "ROADBLOCK_SPIKESTRIP_HIT";

        private static readonly Random Random = new();
        private static readonly ILocalizer Localizer = IoC.Instance.GetInstance<ILocalizer>();
        private static readonly ISpikeStripDispatcher SpikeStripDispatcher = IoC.Instance.GetInstance<ISpikeStripDispatcher>();
        private static readonly IModelProvider ModelProvider = IoC.Instance.GetInstance<IModelProvider>();

        private float _lastKnownDistanceToRoadblock = 9999f;

        protected AbstractPursuitRoadblock(RoadblockData roadblockData, Road street, Vehicle targetVehicle, float targetHeading, ERoadblockFlags flags)
            : base(street, GetMainBarrier(roadblockData), GetSecondaryBarrier(roadblockData), targetHeading, GetLightSources(roadblockData), flags)
        {
            Assert.NotNull(targetVehicle, "targetVehicle cannot be null");
            RoadblockData = roadblockData;
            TargetVehicle = targetVehicle;
            ChaseVehicleBarrier = GetChaseVehicleBarrier(roadblockData);
            RoadblockStateChanged += OnStateChanged;

            Initialize();
        }

        #region Properties

        /// <inheritdoc />
        [CanBeNull]
        public Vehicle TargetVehicle { get; }

        /// <inheritdoc />
        public event RoadblockEvents.RoadblockCopsJoiningPursuit RoadblockCopsJoiningPursuit;

        /// <summary>
        /// The roadblock config data which determines most of the setup.
        /// </summary>
        protected RoadblockData RoadblockData { get; }

        /// <summary>
        /// The barrier model for the chase vehicle.
        /// </summary>
        protected BarrierModel ChaseVehicleBarrier { get; }

        /// <summary>
        /// The backup unit type of the roadblock.
        /// </summary>
        protected EBackupUnit BackupUnit { get; }

        /// <summary>
        /// Verify if the <see cref="TargetVehicle"/> instance is invalidated by the game.
        /// This might be the case by the pursuit suddenly being (forcefully) ended.
        /// </summary>
        private bool IsVehicleInstanceInvalid => TargetVehicle == null || !TargetVehicle.IsValid();

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void CreatePreview()
        {
            base.CreatePreview();
            DoInternalDebugPreviewCreation();
        }

        /// <inheritdoc />
        public override bool Spawn()
        {
            var result = base.Spawn();
            Monitor();

            return result;
        }

        /// <inheritdoc />
        public override void Release(bool releaseAll = false)
        {
            // verify if the roadblock is still active
            // otherwise, we cannot release the entities
            if (State != ERoadblockState.Active)
            {
                Logger.Trace($"Unable to release roadblock instance, instance is not active for {this}");
                return;
            }

            InvokeCopsJoiningPursuit(releaseAll);
            base.Release(releaseAll);
        }

        public override string ToString()
        {
            return $"SpikeStripsEnabled: {Flags.HasFlag(ERoadblockFlags.EnableSpikeStrips)}, {base.ToString()}";
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
        protected abstract IPursuitRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights);

        /// <summary>
        /// Retrieve a list of cops who will join the pursuit.
        /// </summary>
        /// <param name="releaseAll"></param>
        /// <returns>Returns the cops joining the pursuit.</returns>
        protected virtual IList<Ped> RetrieveCopsJoiningThePursuit(bool releaseAll)
        {
            var copsJoining = new List<Ped>();

            if (IsAllowedToJoinPursuit() || releaseAll)
            {
                foreach (var slot in InternalSlots)
                {
                    try
                    {
                        var slotCops = releaseAll ? slot.Cops : slot.CopsJoiningThePursuit;
                        copsJoining.AddRange(slotCops.Select(x => x.GameInstance));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to retrieve slot cops joining the pursuit for {slot}, {ex.Message}", ex);
                        GameUtils.DisplayNotificationDebug("~r~Cops are unable to join the pursuit");
                    }
                }

                Logger.Debug($"A total of {copsJoining.Count} cops are joining the pursuit for {this}");
            }
            else
            {
                Logger.Debug($"Cops are not allowed to join pursuit for {this}");
            }

            return copsJoining;
        }

        /// <inheritdoc />
        protected override void InitializeScenery()
        {
            var direction = MathHelper.ConvertHeadingToDirection(MathHelper.NormalizeHeading(Heading - 180));
            var conePositions = new List<Vector3>
            {
                Position,
                Road.RightSide,
                Road.LeftSide
            };

            foreach (var position in conePositions)
            {
                Instances.Add(BarrierFactory.Create(ModelProvider.FindModelByScriptName<BarrierModel>(Barrier.BigConeStripesScriptName),
                    position + direction * 3.5f));
            }
        }

        /// <inheritdoc />
        protected override void InitializeLights()
        {
            Instances.AddRange(RoadblockLightSources()
                .SelectMany(x => LightSourceFactory.Create(x, this)));
        }

        /// <inheritdoc />
        protected override IReadOnlyList<IPursuitRoadblockSlot> CreateRoadblockSlots(IReadOnlyList<Road.Lane> lanesToBlock)
        {
            Road.Lane spikeStripLane = null;

            if (Flags.HasFlag(ERoadblockFlags.EnableSpikeStrips))
            {
                spikeStripLane = lanesToBlock[Random.Next(lanesToBlock.Count)];
                Logger.Trace($"Adding spike strip on lane {spikeStripLane}");
            }

            return lanesToBlock
                .Select(lane => DoInternalSlotCreation(lane, spikeStripLane))
                .ToList();
        }

        /// <summary>
        /// Create a chase vehicle for this roadblock.
        /// The chase vehicle will be created on the right side of the road.
        /// </summary>
        protected void CreateChaseVehicle()
        {
            LspdfrHelper.CreateBackupUnit(Road.RightSide, TargetHeading + 25, BackupUnit, 1, out var vehicle, out var cops);

            vehicle.Position = Road.RightSide + ChaseVehiclePositionDirection(vehicle.Model);
            vehicle.Heading = TargetHeading + 25;
            EntityUtils.PlaceVehicleOnTheGround(vehicle.GameInstance);
            Instances.Add(vehicle);

            Instances.AddRange(cops);

            // create buffer barrels behind the vehicle
            CreateChaseVehicleBufferBarrels(vehicle.Position);
        }

        /// <summary>
        /// Execute the spawn actions for the chase vehicle.
        /// </summary>
        protected void SpawnChaseVehicleActions()
        {
            var vehicle = Instances
                .Where(x => x.Type == EEntityType.CopVehicle)
                .Select(x => (ARVehicle)x)
                .Select(x => x.GameInstance)
                .First();

            Instances
                .Where(x => x.Type == EEntityType.CopPed)
                .Select(x => (ARPed)x)
                .Select(x => x.GameInstance)
                .ToList()
                .ForEach(x => x.WarpIntoVehicle(vehicle, (int)VehicleSeat.Driver));
        }

        protected List<LightModel> SlotLightSources()
        {
            return LightSources
                .Where(x => (x.Light.Flags & (ELightSourceFlags.RoadLeft | ELightSourceFlags.RoadRight)) == 0)
                .ToList();
        }

        protected List<LightModel> RoadblockLightSources()
        {
            return LightSources
                .Where(x => (x.Light.Flags & ELightSourceFlags.Lane) == 0)
                .ToList();
        }

        protected EBackupUnit RetrieveBackupUnitType()
        {
            return ChanceProvider.Retrieve(RoadblockData.Units).Type;
        }

        /// <summary>
        /// Get valid cop instances from this roadblock.
        /// These instances are from the <see cref="IRoadblock"/> itself and not from any slots.
        /// </summary>
        /// <returns>Returns a list of valid cop instances.</returns>
        protected List<ARPed> GetValidCopInstances()
        {
            return Instances
                .Where(x => x.Type == EEntityType.CopPed)
                .Where(x => x is { IsInvalid: false })
                .Select(x => (ARPed)x)
                .ToList();
        }

        /// <summary>
        /// Retrieve the chase vehicle cop instances.
        /// This also releases all chase vehicle instances from the <see cref="IRoadblock"/>.
        /// </summary>
        /// <returns>Returns the instances to chase the suspect.</returns>
        protected IList<Ped> RetrieveAndReleaseChaseVehicle()
        {
            // only the chase vehicle will join the pursuit
            var cops = GetValidCopInstances();
            Instances.RemoveAll(x => x.Type == EEntityType.CopVehicle);
            Instances.RemoveAll(x => cops.Contains(x));

            return cops
                .Select(x => x.GameInstance)
                .ToList();
        }

        /// <summary>
        /// Indicate that the cops of this roadblock can join the pursuit.
        /// </summary>
        /// <param name="releaseAll"></param>
        protected void InvokeCopsJoiningPursuit(bool releaseAll)
        {
            switch (State)
            {
                case ERoadblockState.Bypassed when !Flags.HasFlag(ERoadblockFlags.JoinPursuitOnBypass):
                case ERoadblockState.Hit when !Flags.HasFlag(ERoadblockFlags.JoinPursuitOnHit):
                    return;
                default:
                    RoadblockCopsJoiningPursuit?.Invoke(this, RetrieveCopsJoiningThePursuit(releaseAll));
                    break;
            }
        }

        private IPursuitRoadblockSlot DoInternalSlotCreation(Road.Lane lane, Road.Lane spikeStripLane)
        {
            var slot = lane == spikeStripLane
                ? CreateSpikeStripSlot(lane, Heading, TargetVehicle, Flags.HasFlag(ERoadblockFlags.EnableLights))
                : CreateSlot(lane, Heading, TargetVehicle, Flags.HasFlag(ERoadblockFlags.EnableLights));

            slot.RoadblockSlotHit += OnRoadblockSlotHit;

            return slot;
        }

        private void OnRoadblockSlotHit(IRoadblockSlot slot, ERoadblockHitType type)
        {
            // verify if the hit should be ignored
            if (State == ERoadblockState.Hit || !Flags.HasFlag(ERoadblockFlags.DetectHit))
                return;

            Game.DisplayNotification(Localizer[LocalizationKey.RoadblockHasBeenHit]);
            BlipFlashNewState(Color.Green);
            if (Flags.HasFlag(ERoadblockFlags.JoinPursuitOnHit))
                Release();
            UpdateState(ERoadblockState.Hit);
            LspdfrUtils.PlayScannerAudioNonBlocking(type == ERoadblockHitType.SpikeStrip ? AudioSpikeStripHit : AudioRoadblockHit);
            Logger.Info("Roadblock has been hit by the suspect");
        }

        private void CreateChaseVehicleBufferBarrels(Vector3 chasePosition)
        {
            var rowPosition = chasePosition + MathHelper.ConvertHeadingToDirection(TargetHeading - 180) * 4f;
            var nextPositionDistance = ChaseVehicleBarrier.Width + ChaseVehicleBarrier.Spacing;
            var nextPositionDirection = MathHelper.ConvertHeadingToDirection(TargetHeading - 90);
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(TargetHeading + 90) * (nextPositionDistance * 2f);

            for (var i = 0; i < 5; i++)
            {
                Instances.Add(BarrierFactory.Create(ChaseVehicleBarrier, startPosition, TargetHeading));
                startPosition += nextPositionDirection * nextPositionDistance;
            }
        }

        private Vector3 ChaseVehiclePositionDirection(Model vehicleModel)
        {
            return MathHelper.ConvertHeadingToDirection(TargetHeading) * 15f +
                   MathHelper.ConvertHeadingToDirection(Heading - 90) * (vehicleModel.Dimensions.X / 2);
        }

        private void Monitor()
        {
            GameUtils.NewSafeFiber(() =>
            {
                while (State == ERoadblockState.Active)
                {
                    try
                    {
                        if (Flags.HasFlag(ERoadblockFlags.DetectBypass))
                            VerifyIfRoadblockIsBypassed();
                        VerifyRoadblockCopKilled();

                        // verify if the target vehicle instance is still valid
                        // if not, set the state to invalid for this roadblock
                        if (IsVehicleInstanceInvalid)
                            InvalidateTheRoadblock();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"An error occurred while monitoring the roadblock, {ex.Message}", ex);
                    }

                    GameFiber.Yield();
                }
            }, $"{GetType()}.Monitor");
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
                if (Flags.HasFlag(ERoadblockFlags.JoinPursuitOnBypass))
                    Release();
                UpdateState(ERoadblockState.Bypassed);
                Logger.Info("Roadblock has been bypassed");
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

        private void BlipFlashNewState(Color color)
        {
            if (Blip == null)
                return;

            Blip.Color = color;
            Blip.Flash(500, BlipFlashDuration);
        }

        private void InvalidateTheRoadblock()
        {
            Logger.Warn($"Unable to verify of the roadblock status (bypass/hit), the vehicle instance is no longer valid for {this}");
            // invalidate the roadblock so it can be cleaned up
            // this will also stop the monitor from running as we're not able
            // to determine any status anymore
            UpdateState(ERoadblockState.Invalid);
        }

        private IPursuitRoadblockSlot CreateSpikeStripSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool addLights)
        {
            return new SpikeStripSlot(SpikeStripDispatcher, Road, lane, targetVehicle, heading, addLights);
        }

        private void OnStateChanged(IRoadblock roadblock, ERoadblockState newState)
        {
            // if the audio is disabled for this roadblock
            // then ignore the state change
            if (!Flags.HasFlag(ERoadblockFlags.PlayAudio))
                return;

            GameUtils.NewSafeFiber(() =>
            {
                switch (newState)
                {
                    case ERoadblockState.Active:
                        Game.DisplayNotification(Localizer[LocalizationKey.RoadblockDispatchedAt, World.GetStreetName(Position)]);
                        LspdfrUtils.PlayScannerAudioNonBlocking(AudioRoadblockDeployed);
                        break;
                    case ERoadblockState.Bypassed:
                        Game.DisplayNotification(Localizer[LocalizationKey.RoadblockHasBeenBypassed]);
                        PlayBypassedAudio();
                        break;
                }
            }, "PursuitRoadblock.OnStateChanged");
        }

        private void PlayBypassedAudio()
        {
            LspdfrUtils.PlayScannerAudioNonBlocking(Flags.HasFlag(ERoadblockFlags.EnableSpikeStrips) ? AudioSpikeStripBypassed : AudioRoadblockBypassed);
        }

        private static BarrierModel GetMainBarrier(RoadblockData roadblockData)
        {
            return ModelProvider.FindModelByScriptName<BarrierModel>(roadblockData.MainBarrier);
        }

        private static BarrierModel GetSecondaryBarrier(RoadblockData roadblockData)
        {
            return !string.IsNullOrWhiteSpace(roadblockData.SecondaryBarrier)
                ? ModelProvider.FindModelByScriptName<BarrierModel>(roadblockData.SecondaryBarrier)
                : BarrierModel.None;
        }

        private static BarrierModel GetChaseVehicleBarrier(RoadblockData roadblockData)
        {
            return !string.IsNullOrWhiteSpace(roadblockData.ChaseVehicleBarrier)
                ? ModelProvider.FindModelByScriptName<BarrierModel>(roadblockData.ChaseVehicleBarrier)
                : BarrierModel.None;
        }

        private static List<LightModel> GetLightSources(RoadblockData roadblockData)
        {
            return roadblockData.Lights
                .Select(x => ModelProvider.FindModelByScriptName<LightModel>(x))
                .ToList();
        }

        [Conditional("DEBUG")]
        private void DoInternalDebugPreviewCreation()
        {
            GameUtils.NewSafeFiber(() =>
            {
                Logger.Trace($"Creating roadblock debug preview for {GetType()}");
                var color = PursuitIndicatorColor();
                var copsJoiningThePursuit = RetrieveCopsJoiningThePursuit(false);

                while (IsPreviewActive)
                {
                    var remainingCops = Slots
                        .SelectMany(x => x.Cops)
                        .Where(x => !copsJoiningThePursuit.Contains(x.GameInstance))
                        .Select(x => x.GameInstance)
                        .ToList();

                    foreach (var ped in copsJoiningThePursuit)
                    {
                        GameUtils.CreateMarker(ped.Position, EMarkerType.MarkerTypeUpsideDownCone, color, 1f, 1f, false);
                    }

                    foreach (var ped in remainingCops)
                    {
                        GameUtils.CreateMarker(ped.Position, EMarkerType.MarkerTypeUpsideDownCone, Color.DarkRed, 1f, 1f, false);
                    }

                    GameFiber.Yield();
                }
            }, "Roadblock.Preview");
        }

        private Color PursuitIndicatorColor()
        {
            var color = Color.DarkRed;

            if (Flags.HasFlag(ERoadblockFlags.JoinPursuit))
            {
                color = Color.Lime;
            }
            else if (Flags.HasFlag(ERoadblockFlags.JoinPursuitOnBypass))
            {
                color = Color.Gold;
            }
            else if (Flags.HasFlag(ERoadblockFlags.JoinPursuitOnHit))
            {
                color = Color.Coral;
            }

            return color;
        }

        #endregion
    }
}