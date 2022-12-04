using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Vehicles;
using JetBrains.Annotations;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    /// <summary>
    /// Abstract implementation of the <see cref="IRoadblockSlot"/>.
    /// This slot defines the entities and scenery items used within the <see cref="IRoadblock"/>.
    /// </summary>
    /// <remarks>Make sure that the <see cref="Initialize"/> method is called within the constructor after all properties/fields are set for the slot.</remarks>
    public abstract class AbstractRoadblockSlot : IRoadblockSlot
    {
        private const float DefaultVehicleWidth = 2.5f;
        private const float DefaultVehicleLength = 4f;
        protected const int VehicleHeadingMaxOffset = 25;
        protected static readonly Random Random = new();

        protected readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        protected readonly IGame Game = IoC.Instance.GetInstance<IGame>();

        private readonly bool _shouldAddLights;

        protected AbstractRoadblockSlot(Road.Lane lane, BarrierModel mainBarrier, BarrierModel secondaryBarrier, EBackupUnit backupType, float heading,
            bool shouldAddLights, bool recordVehicleCollisions, float offset = 0f)
        {
            Assert.NotNull(lane, "lane cannot be null");
            Assert.NotNull(mainBarrier, "barrierType cannot be null");
            Assert.NotNull(backupType, "backupType cannot be null");
            Assert.NotNull(heading, "heading cannot be null");
            Lane = lane;
            MainBarrier = mainBarrier;
            SecondaryBarrier = secondaryBarrier;
            BackupType = backupType;
            Heading = heading;
            RecordVehicleCollisions = recordVehicleCollisions;
            Offset = offset;
            _shouldAddLights = shouldAddLights;
        }

        #region Properties

        /// <inheritdoc />
        public Vector3 Position => Lane.Position;

        /// <inheritdoc />
        public Vector3 OffsetPosition => Position + MathHelper.ConvertHeadingToDirection(Heading) * Offset;

        /// <inheritdoc />
        public float Heading { get; }

        /// <inheritdoc />
        public float VehicleLength => VehicleModel != null
            ? VehicleModel.Value.Dimensions.Y
            : DefaultVehicleLength;

        /// <inheritdoc />
        public ARVehicle Vehicle => VehicleInstance;

        /// <inheritdoc />
        public Road.Lane Lane { get; }

        /// <inheritdoc />
        public IList<ARPed> Cops => ValidCopInstances.ToList();

        /// <summary>
        /// The main barrier that is used within this slot as first row.
        /// </summary>
        public BarrierModel MainBarrier { get; }

        /// <summary>
        /// The secondary barrier that is used within this slot as second row.
        /// </summary>
        public BarrierModel SecondaryBarrier { get; }

        public EBackupUnit BackupType { get; }

        /// <summary>
        /// The instances of this slot.
        /// </summary>
        protected List<IARInstance<Entity>> Instances { get; } = new();

        /// <summary>
        /// The vehicle model of this slot.
        /// </summary>
        [CanBeNull]
        protected Model? VehicleModel { get; }

        /// <summary>
        /// The indication if the spawned vehicle should record collisions.
        /// </summary>
        protected bool RecordVehicleCollisions { get; }

        /// <summary>
        /// The offset of the position in regards to the node.
        /// </summary>
        protected float Offset { get; }

        /// <summary>
        /// The total number of cops for this slot.
        /// </summary>
        protected int? NumberOfCops { get; set; }

        /// <summary>
        /// Get the AR vehicle instance of this slot.
        /// </summary>
        protected ARVehicle VehicleInstance => Instances
            .Where(x => x.Type == EEntityType.CopVehicle)
            .Select(x => (ARVehicle)x)
            .FirstOrDefault();

        /// <summary>
        /// Get the cop ped instance(s) of this slot.
        /// </summary>
        protected IEnumerable<ARPed> CopInstances
        {
            get
            {
                return Instances
                    .Where(x => x.Type == EEntityType.CopPed)
                    .Select(x => (ARPed)x);
            }
        }

        /// <summary>
        /// The list of valid cop instance slots.
        /// </summary>
        protected IEnumerable<ARPed> ValidCopInstances => Instances
            .Where(x => x.Type == EEntityType.CopPed)
            .Select(x => (ARPed)x)
            .Where(x => x is { IsInvalid: false });

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive { get; private set; }

        /// <inheritdoc />
        public void CreatePreview()
        {
            if (IsPreviewActive)
                return;

            IsPreviewActive = true;
            Logger.Debug($"Creating a total of {Instances.Count} instances for the roadblock slot preview");
            Instances.ForEach(x => DoSafeOperation(x.CreatePreview, $"create instance slot {x} preview"));
            DrawRoadblockDebugInfo();
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            if (!IsPreviewActive)
                return;

            Instances.ForEach(x => DoSafeOperation(x.DeletePreview, $"delete instance slot {x} preview"));
            IsPreviewActive = false;
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public virtual void Dispose()
        {
            DeletePreview();
            Instances.ForEach(x => DoSafeOperation(x.Dispose, $"dispose instance slot {x}"));
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"Number of {nameof(Instances)}: {Instances.Count}, {nameof(Position)}: {Position}, {nameof(OffsetPosition)}: {OffsetPosition}, {nameof(Heading)}: {Heading}, " +
                $"{nameof(MainBarrier)}: {MainBarrier}, {nameof(SecondaryBarrier)}: {SecondaryBarrier}, {nameof(BackupType)}: {BackupType}";
        }

        /// <inheritdoc />
        public virtual void Spawn()
        {
            if (IsPreviewActive)
                DeletePreview();
        }

        /// <inheritdoc />
        public void ModifyVehiclePosition(Vector3 newPosition)
        {
            Assert.NotNull(newPosition, "newPosition cannot be null");
            var vehicleInstance = Instances
                .Where(x => x.Type == EEntityType.CopVehicle)
                .Select(x => (ARVehicle)x)
                .First();

            vehicleInstance.Position = newPosition;
            EntityUtils.PlaceVehicleOnTheGround(vehicleInstance.GameInstance);
        }

        /// <inheritdoc />
        public virtual void Release(bool releaseAll = false)
        {
            DoInternalRelease(Cops);
        }

        /// <inheritdoc />
        public void WarpInVehicle()
        {
            if (Vehicle == null || Vehicle.IsInvalid)
            {
                Logger.Warn("Unable to warp cops into vehicle, vehicle instance is invalid");
                return;
            }

            CopInstances.ToList()
                .ForEach(x => x.WarpIntoVehicle(Vehicle.GameInstance, Vehicle.GameInstance.Driver == null ? EVehicleSeat.Driver : EVehicleSeat.Any));
        }

        #endregion

        #region Functions

        /// <summary>
        /// Initialize this roadblock slot.
        /// This will create all entities/scenery items of this slot.
        /// </summary>
        protected void Initialize()
        {
            if (!LspdfrHelper.CreateBackupUnit(CalculateVehiclePosition(), CalculateVehicleHeading(), BackupType, NumberOfCops, out var vehicle, out var cops,
                    RecordVehicleCollisions))
            {
                Logger.Error("Unable to initialize roadblock slot, LSPDFR backup unit creation failed");
                return;
            }

            InitializeVehicleSlot(vehicle);
            InitializeCops(cops);
            InitializeScenery();

            if (!MainBarrier.IsNone)
                InitializeBarriers(MainBarrier, 2f);

            if (!SecondaryBarrier.IsNone)
                InitializeBarriers(SecondaryBarrier, -4f);

            if (_shouldAddLights)
                InitializeLights();
        }

        /// <summary>
        /// Calculate the position for cops which is behind the vehicle.
        /// This calculation is based on the width of the vehicle model.
        /// </summary>
        /// <returns>Returns the position behind the vehicle.</returns>
        protected virtual Vector3 CalculatePositionBehindVehicle()
        {
            return OffsetPosition + MathHelper.ConvertHeadingToDirection(Heading) * (GetVehicleWidth() + 0.5f);
        }

        /// <summary>
        /// Calculate the facing heading of the cop instances.
        /// </summary>
        /// <returns>Returns the heading for the cop peds.</returns>
        protected virtual float CalculateCopHeading()
        {
            return Heading - 180;
        }

        /// <summary>
        /// Initialize the scenery props of this slot.
        /// </summary>
        protected abstract void InitializeScenery();

        /// <summary>
        /// Initialize the light props of this slots.
        /// </summary>
        protected abstract void InitializeLights();

        /// <summary>
        /// Calculate the heading of the vehicle for this slot.
        /// </summary>
        /// <returns>Returns the heading for the vehicle.</returns>
        protected virtual float CalculateVehicleHeading()
        {
            // because nobody is perfect and we want a natural look on the roadblocks
            // we slightly create an offset in the heading for each created vehicle
            return Heading + Random.Next(90 - VehicleHeadingMaxOffset, 91 + VehicleHeadingMaxOffset);
        }

        /// <summary>
        /// Calculate the position of the vehicle for this slot.
        /// </summary>
        /// <returns>Returns the position for the vehicle placement.</returns>
        protected virtual Vector3 CalculateVehiclePosition()
        {
            return OffsetPosition;
        }

        /// <summary>
        /// Initialize the cop instances of this slot.
        /// </summary>
        protected virtual void InitializeCops(IEnumerable<ARPed> cops)
        {
            var pedSpawnPosition = CalculatePositionBehindVehicle();
            var pedHeading = CalculateCopHeading();

            foreach (var cop in cops)
            {
                cop.PlaceOnGroundAt(pedSpawnPosition);
                cop.Heading = pedHeading;
                Instances.Add(cop);

                pedSpawnPosition += MathHelper.ConvertHeadingToDirection(Heading + 90) * 1.5f;
            }
        }

        /// <summary>
        /// Release the given list of cops back to LSPDFR.
        /// </summary>
        /// <param name="cops">The list of cops to release.</param>
        protected void DoInternalRelease(IList<ARPed> cops)
        {
            Logger.Trace($"Releasing {GetType()} {this}");
            Instances
                .Where(x => x.Type == EEntityType.CopVehicle)
                .ToList()
                .ForEach(x => Instances.Remove(x));
            Instances.RemoveAll(x => cops.Any(instance => x == instance));
            Logger.Trace($"{GetType()} state after release {this}");

            RoadblockHelpers.ReleaseInstancesToLspdfr(cops, Vehicle);
        }

        /// <summary>
        /// Get the width of the vehicle model.
        /// When no vehicle model is present, it will use a default value.
        /// </summary>
        /// <returns>Returns the vehicle model width.</returns>
        private float GetVehicleWidth()
        {
            return VehicleModel != null
                ? VehicleModel.Value.Dimensions.X
                : DefaultVehicleWidth;
        }

        private void InitializeVehicleSlot(ARVehicle vehicle)
        {
            if (BackupType == EBackupUnit.None)
            {
                vehicle.Dispose();
            }
            else
            {
                vehicle.Heading = CalculateVehicleHeading();
                EntityUtils.PlaceVehicleOnTheGround(vehicle.GameInstance);
                Instances.Add(vehicle);
            }
        }

        private void InitializeBarriers(BarrierModel barrierModel, float roadOffset)
        {
            Logger.Trace($"Initializing roadblock slot barriers for {{{barrierModel}}} with offset {roadOffset}");
            var rowPosition = OffsetPosition + MathHelper.ConvertHeadingToDirection(Heading - 180) * roadOffset;
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(Heading + 90) * (Lane.Width / 2 - barrierModel.Width / 2);
            var direction = MathHelper.ConvertHeadingToDirection(Heading - 90);
            var barrierTotalWidth = barrierModel.Spacing + barrierModel.Width;
            var totalBarriers = (int)Math.Floor(Lane.Width / barrierTotalWidth);

            Logger.Trace($"Barrier info: lane width {Lane.Width}, type {barrierModel}, width: {barrierModel.Width}, spacing: {barrierModel.Spacing}");
            Logger.Debug($"Creating a total of {totalBarriers} barriers with type {barrierModel} for the roadblock slot");
            for (var i = 0; i < totalBarriers; i++)
            {
                Instances.Add(CreateBarrier(barrierModel, startPosition, Heading));
                startPosition += direction * barrierTotalWidth;
            }
        }

        private IARInstance<Entity> CreateBarrier(BarrierModel barrierModel, Vector3 position, float heading)
        {
            try
            {
                return BarrierFactory.Create(barrierModel, position, heading);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create barrier of type {barrierModel}, {ex.Message}", ex);
                return null;
            }
        }

        private void DoSafeOperation(Action action, string operation)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to {operation}, {ex.Message}", ex);
            }
        }

        [Conditional("DEBUG")]
        private void DrawRoadblockDebugInfo()
        {
            Logger.Trace("Drawing the roadblock slot debug information within the preview");
            Game.NewSafeFiber(() =>
            {
                var direction = MathHelper.ConvertHeadingToDirection(Heading);
                var position = OffsetPosition + Vector3.WorldUp * 0.25f;

                while (IsPreviewActive)
                {
                    Game.DrawArrow(position, direction, Rotator.Zero, 2f, Color.Yellow);
                    Game.FiberYield();
                }
            }, "IRoadblockSlot.CreatePreview");
        }

        #endregion
    }
}