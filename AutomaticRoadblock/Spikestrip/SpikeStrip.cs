using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.Animation;
using AutomaticRoadblocks.Logging;
using AutomaticRoadblocks.Sound;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Vehicles;
using Rage;
using Rage.Native;
using Object = Rage.Object;

namespace AutomaticRoadblocks.SpikeStrip
{
    /// <summary>
    /// A basic spike strip implementation which implements the <see cref="ISpikeStrip"/>.
    /// This spike strip implements the animations and logic for bursting tires when needed.
    /// </summary>
    public class SpikeStrip : ISpikeStrip
    {
        protected static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        private AnimationExecutor _animation;
        private long _spawnTimeDuration;
        private long _undeployAnimationDuration;
        private long _waitForUndeployCompleted;
        private long _deployAnimationDuration;

        internal SpikeStrip(Road street, ESpikeStripLocation location, float offset)
        {
            Assert.NotNull(street, "road cannot be null");
            Road = street;
            Location = location;
            Lane = CalculatePlacementLane();
            Position = CalculateSpikeStripPosition(offset);
        }

        internal SpikeStrip(Road street, Road.Lane lane, ESpikeStripLocation location, float offset)
        {
            Assert.NotNull(street, "road cannot be null");
            Assert.NotNull(lane, "lane cannot be null");
            Road = street;
            Lane = lane;
            Location = location;
            Position = CalculateSpikeStripPosition(offset);
        }

        #region Properties

        /// <inheritdoc />
        public Vector3 Position { get; }

        /// <inheritdoc />
        public float Heading
        {
            get
            {
                return Location switch
                {
                    ESpikeStripLocation.Left => Road.Node.Heading - 90,
                    _ => Road.Node.Heading + 90
                };
            }
        }

        /// <inheritdoc />
        public ESpikeStripLocation Location { get; }

        /// <inheritdoc />
        public ESpikeStripState State { get; private set; } = ESpikeStripState.Preparing;

        /// <inheritdoc />
        public Object GameInstance { get; private set; }

        /// <inheritdoc />
        public event SpikeStripEvents.SpikeStripStateChanged StateChanged;

        /// <summary>
        /// The road this spike strip is deployed on.
        /// </summary>
        private Road Road { get; }

        /// <summary>
        /// The lane on which the spike strip is placed.
        /// </summary>
        private Road.Lane Lane { get; }

        /// <summary>
        /// Verify if the instance has been invalidated.
        /// </summary>
        private bool IsInvalid => GameInstance == null || !GameInstance.IsValid();

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => Road.IsPreviewActive;

        /// <inheritdoc />
        public void CreatePreview()
        {
            if (IsPreviewActive)
                return;

            GameUtils.NewSafeFiber(() =>
            {
                Logger.Trace($"Creating spike strip preview for {this}");
                Road.CreatePreview();
                DoInternalSpawn();

                if (!IsInvalid)
                {
                    PreviewUtils.TransformToPreview(GameInstance);
                    Logger.Debug($"Created spike strip preview for {this}");
                }

                while (IsPreviewActive)
                {
                    GameUtils.DrawSphere(Position, 0.2f, Color.Red);
                    GameFiber.Yield();
                }
            }, "SpikeStrip.Preview");
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            if (!IsPreviewActive)
                return;

            Road.DeletePreview();
            PreviewUtils.TransformToNormal(GameInstance);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Spawn()
        {
            GameUtils.NewSafeFiber(DoInternalSpawn, "SpikeStrip.Spawn");
        }

        /// <inheritdoc />
        public void Deploy()
        {
            GameUtils.NewSafeFiber(DoInternalDeploy, "SpikeStrip.Deploy");
        }

        /// <inheritdoc />
        public void Undeploy()
        {
            GameUtils.NewSafeFiber(DoInternalUndeploy, "SpikeStrip.Undeploy");
        }

        public override string ToString()
        {
            return
                $"{nameof(Location)}: {Location}, {nameof(Position)}: {Position}, {nameof(Heading)}: {Heading}, {nameof(State)}: {State}, {nameof(Road)}: {Road}";
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            DeletePreview();
            DoInternalCleanup();
        }

        #endregion

        #region Functions

        /// <summary>
        /// Invoked when the given vehicle has hit this spike strip.
        /// </summary>
        /// <param name="vehicle">The vehicle that hit the spike strip.</param>
        /// <param name="wheel">The vehicle's wheel that hit it.</param>
        protected virtual void VehicleHitSpikeStrip(Vehicle vehicle, VehicleWheel wheel)
        {
            Logger.Debug($"Vehicle hit spike strip in state {State}");
            wheel.BurstTire();
        }

        /// <summary>
        /// Process additional verifications while the spike strip is active.
        /// </summary>
        protected virtual void DoAdditionalVerifications()
        {
            // no-op
        }

        /// <summary>
        /// Update the state of the spike strip.
        /// </summary>
        /// <param name="state">The new state of this spike strip.</param>
        protected void UpdateState(ESpikeStripState state)
        {
            State = state;

            // move the state invocation to another thread so it doesn't block the current one
            GameUtils.NewSafeFiber(() => { StateChanged?.Invoke(this, state); }, $"{GetType()}.UpdateState");
        }

        private void DoInternalSpawn()
        {
            if (!IsInvalid)
                return;

            Logger.Trace($"Spawning spike strip {this}");
            var startTime = DateTime.Now.Ticks;
            GameInstance = PropUtils.CreateSpikeStrip(Position, Heading);
            GameInstance.IsPersistent = true;
            _spawnTimeDuration = DateTime.Now.Ticks - startTime;
            DoUndeployAnimation();
        }

        private void DoInternalDeploy()
        {
            var startTime = DateTime.Now.Ticks;
            if (IsInvalid)
                DoInternalSpawn();

            Logger.Trace($"Starting deployment of spike strip {this}");
            var waitForStart = DateTime.Now.Ticks;
            // wait for the spike strip to become available
            while (State != ESpikeStripState.Undeployed)
            {
                GameFiber.Yield();
            }

            _waitForUndeployCompleted = DateTime.Now.Ticks;
            Logger.Debug($"Deploying spike strip {this}");
            UpdateState(ESpikeStripState.Deploying);
            StopCurrentAnimation();
            DoDeployAnimation();
            UpdateState(ESpikeStripState.Deployed);
            StartMonitor();
            LogPerformance(waitForStart, startTime);
        }

        private void DoInternalUndeploy()
        {
            if (IsInvalid || State is not (ESpikeStripState.Deployed or ESpikeStripState.Bypassed or ESpikeStripState.Hit))
                return;

            Logger.Trace($"Undeploying spike strip {this}");
            StopCurrentAnimation();
            DoUndeployAnimation();
        }

        private void DoInternalCleanup()
        {
            if (GameInstance == null)
                return;

            UpdateState(ESpikeStripState.Disposed);
            EntityUtils.Remove(GameInstance);
        }

        private void StartMonitor()
        {
            GameUtils.NewSafeFiber(() =>
            {
                Logger.Debug("Starting spike strip monitor");
                while (State is not (ESpikeStripState.Preparing or ESpikeStripState.Undeployed or ESpikeStripState.Disposed))
                {
                    DoNearbyVehiclesCheck();
                    DoAdditionalVerifications();
                    GameFiber.Yield();
                }

                Logger.Debug("Spike strip monitor completed");
            }, $"{GetType()}.Monitor");
        }

        private void DoNearbyVehiclesCheck()
        {
            foreach (var vehicle in NearbyVehicles())
            {
                foreach (var wheelIndex in AllWheels())
                {
                    if (!vehicle.HasBone(wheelIndex.BoneName) || IsVehicleTireBurst(vehicle, wheelIndex, false))
                        continue;

                    var wheel = vehicle.Wheels[wheelIndex.Index];

                    if (IsTouchingTheInstance(wheel.LastContactPoint))
                        VehicleHitSpikeStrip(vehicle, wheel);
                }
            }
        }

        private IEnumerable<Vehicle> NearbyVehicles()
        {
            return World
                .GetEntities(Position, PropUtils.Models.SpikeStrip.Dimensions.Y, GetEntitiesFlags.ConsiderGroundVehicles)
                .OfType<Vehicle>()
                .Where(x => x.IsValid());
        }

        // Credits to PNWParksFan (see discord knowledge base)

        private bool IsTouchingTheInstance(Vector3 position)
        {
            if (IsInvalid)
                return false;

            var instancePosition = GameInstance.Position;
            var orientation = GameInstance.Orientation;
            var size = PropUtils.Models.SpikeStrip.Dimensions;
            var compareAgainst = position - instancePosition;
            orientation.GetAxes(right: out var xRot, out var yRot, out var zRot);

            var px = compareAgainst.ProjectOnTo(xRot).Length();
            var py = compareAgainst.ProjectOnTo(yRot).Length();
            var pz = compareAgainst.ProjectOnTo(zRot).Length();

            return px * 2 < size.X &&
                   py * 2 < size.Y &&
                   pz * 2 < size.Z;
        }

        private void StopCurrentAnimation()
        {
            _animation?.Stop();
        }

        private void DoDeployAnimation()
        {
            if (IsInvalid)
                return;

            Logger.Trace($"Playing spike strip deploy animation for {this}");
            var startTime = DateTime.Now.Ticks;
            SoundHelper.PlaySound(GameInstance, Sounds.StingerDrop, Sounds.StingerDropRef);
            _animation = AnimationHelper.PlayAnimation(GameInstance, Animations.Dictionaries.StingerDictionary, Animations.SpikeStripDeploy,
                AnimationFlags.None);
            _animation.Speed = 2.5f;
            _animation.WaitForCompletion();
            _deployAnimationDuration = DateTime.Now.Ticks - startTime;
        }

        private void DoUndeployAnimation()
        {
            if (IsInvalid)
                return;

            var startTime = DateTime.Now.Ticks;
            SoundHelper.PlaySound(GameInstance, Sounds.StingerDrop, Sounds.StingerDropRef);
            _animation = AnimationHelper.PlayAnimation(GameInstance, Animations.Dictionaries.StingerDictionary, Animations.SpikeStripIdleUndeployed,
                AnimationFlags.StayInEndFrame);
            PropUtils.PlaceCorrectlyOnGround(GameInstance);
            _animation.WaitForCompletion();
            UpdateState(ESpikeStripState.Undeployed);
            _undeployAnimationDuration = DateTime.Now.Ticks - startTime;
        }

        private Vector3 CalculateSpikeStripPosition(float offset)
        {
            var position = Location switch
            {
                ESpikeStripLocation.Left => Road.LeftSide,
                ESpikeStripLocation.Middle => Road.Position,
                _ => Road.RightSide
            };

            return position
                   + MathHelper.ConvertHeadingToDirection(Heading) * (Lane.Width / 2)
                   + MathHelper.ConvertHeadingToDirection(Road.Node.Heading) * offset;
        }

        private Road.Lane CalculatePlacementLane()
        {
            var positionToMatch = Location switch
            {
                ESpikeStripLocation.Left => Road.LeftSide,
                ESpikeStripLocation.Middle => Road.Position,
                _ => Road.RightSide
            };

            return Road.LaneClosestTo(positionToMatch);
        }

        private void LogPerformance(long waitForStart, long startTime)
        {
            var spawnTime = _spawnTimeDuration / TimeSpan.TicksPerMillisecond;
            var undeployTime = _undeployAnimationDuration / TimeSpan.TicksPerMillisecond;
            var waitForUndeploy = (waitForStart - _waitForUndeployCompleted) / TimeSpan.TicksPerMillisecond;
            var deployAnimation = _deployAnimationDuration / TimeSpan.TicksPerMillisecond;
            var totalDuration = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
            Logger.Trace("--- Spike strip performance stats ---\n" +
                         $"Total time: {totalDuration}ms\n" +
                         $"Spawn time: {spawnTime}ms\n" +
                         $"Undeploy anim: {undeployTime}ms\n" +
                         $"Wait for: {waitForUndeploy}ms\n" +
                         $"Deploy anim: {deployAnimation}ms");
        }

        private static bool IsVehicleTireBurst(Vehicle vehicle, EVehicleWheel wheel, bool onRim)
        {
            // VEHICLE::IS_VEHICLE_TYRE_BURST
            return NativeFunction.CallByHash<bool>(0xBA291848A0815CA9, vehicle, wheel.Index, onRim);
        }

        private static IEnumerable<EVehicleWheel> AllWheels()
        {
            return new[]
            {
                EVehicleWheel.LeftFront,
                EVehicleWheel.RightFront,
                EVehicleWheel.LeftMiddle,
                EVehicleWheel.RightMiddle,
                EVehicleWheel.LeftRear,
                EVehicleWheel.RightRear,
            };
        }

        private static string TimeToColor(long timeTaken)
        {
            return timeTaken < 500 ? "g" : "r";
        }

        #endregion
    }
}