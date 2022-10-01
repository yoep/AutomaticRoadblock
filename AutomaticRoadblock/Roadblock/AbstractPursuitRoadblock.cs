using System;
using System.Collections.Generic;
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
using JetBrains.Annotations;
using LSPD_First_Response.Engine.Scripting.Entities;
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
        private const string AudioRoadblockDeployed = "ROADBLOCK_DEPLOYED";
        private const string AudioRoadblockBypassed = "ROADBLOCK_BYPASSED";
        private const string AudioRoadblockHit = "ROADBLOCK_HIT";

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

        /// <summary>
        /// The roadblock config data which determines most of the setup.
        /// </summary>
        protected RoadblockData RoadblockData { get; }

        /// <summary>
        /// Get the target vehicle of this roadblock.
        /// </summary>
        [CanBeNull]
        protected Vehicle TargetVehicle { get; }

        /// <summary>
        /// The barrier model for the chase vehicle.
        /// </summary>
        protected BarrierModel ChaseVehicleBarrier { get; }

        /// <summary>
        /// Verify if the <see cref="TargetVehicle"/> instance is invalidated by the game.
        /// This might be the case by the pursuit suddenly being (forcefully) ended.
        /// </summary>
        private bool IsVehicleInstanceInvalid => TargetVehicle == null || !TargetVehicle.IsValid();

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
        protected abstract IRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights);

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
                Instances.Add(new InstanceSlot(EEntityType.Scenery, position + direction * 3.5f, 0f,
                    (conePosition, _) =>
                        BarrierFactory.Create(ModelProvider.RetrieveModelByScriptName<BarrierModel>(Barrier.BigConeStripesScriptName), conePosition)));
            }
        }

        /// <inheritdoc />
        protected override void InitializeLights()
        {
            Instances.AddRange(RoadblockLightSources()
                .SelectMany(x => LightSourceFactory.Create(x, this)));
        }

        /// <inheritdoc />
        protected override IReadOnlyList<IRoadblockSlot> CreateRoadblockSlots(IReadOnlyList<Road.Lane> lanesToBlock)
        {
            Road.Lane spikeStripLane = null;

            if (Flags.HasFlag(ERoadblockFlags.EnableSpikeStrips))
            {
                spikeStripLane = lanesToBlock[Random.Next(lanesToBlock.Count)];
                Logger.Trace($"Adding spike strip on lane {spikeStripLane}");
            }

            return lanesToBlock
                .Select(lane => lane == spikeStripLane
                    ? CreateSpikeStripSlot(lane, Heading, TargetVehicle, Flags.HasFlag(ERoadblockFlags.EnableLights))
                    : CreateSlot(lane, Heading, TargetVehicle, Flags.HasFlag(ERoadblockFlags.EnableLights)))
                .ToList();
        }

        /// <summary>
        /// Create a chase vehicle for this roadblock.
        /// The chase vehicle will be created on the right side of the road.
        /// </summary>
        protected void CreateChaseVehicle(Model vehicleModel)
        {
            Assert.NotNull(vehicleModel, "vehicleModel cannot be null");
            var roadPosition = Road.RightSide + ChaseVehiclePositionDirection(vehicleModel);

            Instances.AddRange(new[]
            {
                new InstanceSlot(EEntityType.CopVehicle, roadPosition, TargetHeading + 25,
                    (position, heading) => new ARVehicle(vehicleModel, GameUtils.GetOnTheGroundPosition(position), heading)),
                new InstanceSlot(EEntityType.CopPed, roadPosition, TargetHeading,
                    (position, heading) =>
                        new ARPed(LspdfrDataHelper.RetrieveCop(RetrieveBackupUnitType(), position), heading))
            });

            // create buffer barrels behind the vehicle
            CreateChaseVehicleBufferBarrels(roadPosition);
        }

        /// <summary>
        /// Execute the spawn actions for the chase vehicle.
        /// </summary>
        protected void SpawnChaseVehicleActions()
        {
            var vehicle = Instances
                .Where(x => x.Type == EEntityType.CopVehicle)
                .Select(x => x.Instance)
                .Select(x => (ARVehicle)x)
                .Select(x => x.GameInstance)
                .First();

            Instances
                .Where(x => x.Type == EEntityType.CopPed)
                .Select(x => x.Instance)
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

        protected Model RetrieveVehicleModel()
        {
            return LspdfrDataHelper.RetrieveVehicleModel(RetrieveBackupUnitType(), OffsetPosition);
        }

        protected EBackupUnit RetrieveBackupUnitType()
        {
            return ChanceProvider.Retrieve(RoadblockData.Units).Type;
        }

        private void CreateChaseVehicleBufferBarrels(Vector3 chasePosition)
        {
            var rowPosition = chasePosition + MathHelper.ConvertHeadingToDirection(TargetHeading - 180) * 4f;
            var nextPositionDistance = ChaseVehicleBarrier.Width + ChaseVehicleBarrier.Spacing;
            var nextPositionDirection = MathHelper.ConvertHeadingToDirection(TargetHeading - 90);
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(TargetHeading + 90) * (nextPositionDistance * 2f);

            for (var i = 0; i < 5; i++)
            {
                Instances.Add(new InstanceSlot(EEntityType.Scenery, startPosition, TargetHeading,
                    (position, heading) => BarrierFactory.Create(ChaseVehicleBarrier, position, heading)));
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
            Game.NewSafeFiber(() =>
            {
                while (State == ERoadblockState.Active)
                {
                    try
                    {
                        if (Flags.HasFlag(ERoadblockFlags.DetectBypass))
                            VerifyIfRoadblockIsBypassed();
                        if (Flags.HasFlag(ERoadblockFlags.DetectHit))
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
                if (Flags.HasFlag(ERoadblockFlags.JoinPursuitOnBypass))
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

            if (!Slots
                    .Where(x => x.Vehicle != null && x.Vehicle.IsValid())
                    .Any(HasBeenDamagedBy))
                return;

            Logger.Debug("Determined that the collision must have been against a roadblock slot");
            BlipFlashNewState(Color.Green);
            if (Flags.HasFlag(ERoadblockFlags.JoinPursuitOnHit))
                Release();
            UpdateState(ERoadblockState.Hit);
            Logger.Info("Roadblock has been hit by the suspect");
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
            Logger.Warn($"Unable to verify of the roadblock status (bypass/hit), the vehicle instance is no longer valid for {this}");
            // invalidate the roadblock so it can be cleaned up
            // this will also stop the monitor from running as we're not able
            // to determine any status anymore
            UpdateState(ERoadblockState.Invalid);
        }

        private IRoadblockSlot CreateSpikeStripSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool addLights)
        {
            return new SpikeStripSlot(SpikeStripDispatcher, Road, lane, targetVehicle, heading, addLights);
        }

        private void OnStateChanged(IRoadblock roadblock, ERoadblockState newState)
        {
            // if the audio is disabled for this roadblock
            // then ignore the state change
            if (!Flags.HasFlag(ERoadblockFlags.PlayAudio))
                return;

            Game.NewSafeFiber(() =>
            {
                switch (newState)
                {
                    case ERoadblockState.Active:
                        Game.DisplayNotification(Localizer[LocalizationKey.RoadblockDispatchedAt, World.GetStreetName(Position)]);
                        LspdfrUtils.PlayScannerAudioNonBlocking(AudioRoadblockDeployed);
                        break;
                    case ERoadblockState.Hit:
                        Game.DisplayNotification(Localizer[LocalizationKey.RoadblockHasBeenHit]);
                        LspdfrUtils.PlayScannerAudioNonBlocking(AudioRoadblockHit);
                        break;
                    case ERoadblockState.Bypassed:
                        Game.DisplayNotification(Localizer[LocalizationKey.RoadblockHasBeenBypassed]);
                        LspdfrUtils.PlayScannerAudioNonBlocking(AudioRoadblockBypassed);
                        break;
                }
            }, "PursuitRoadblock.OnStateChanged");
        }

        private static BarrierModel GetMainBarrier(RoadblockData roadblockData)
        {
            return ModelProvider.RetrieveModelByScriptName<BarrierModel>(roadblockData.MainBarrier);
        }

        private static BarrierModel GetSecondaryBarrier(RoadblockData roadblockData)
        {
            return !string.IsNullOrWhiteSpace(roadblockData.SecondaryBarrier)
                ? ModelProvider.RetrieveModelByScriptName<BarrierModel>(roadblockData.SecondaryBarrier)
                : BarrierModel.None;
        }

        private static BarrierModel GetChaseVehicleBarrier(RoadblockData roadblockData)
        {
            return !string.IsNullOrWhiteSpace(roadblockData.ChaseVehicleBarrier)
                ? ModelProvider.RetrieveModelByScriptName<BarrierModel>(roadblockData.ChaseVehicleBarrier)
                : BarrierModel.None;
        }

        private static List<LightModel> GetLightSources(RoadblockData roadblockData)
        {
            return roadblockData.Lights
                .Select(x => ModelProvider.RetrieveModelByScriptName<LightModel>(x))
                .ToList();
        }

        #endregion
    }
}