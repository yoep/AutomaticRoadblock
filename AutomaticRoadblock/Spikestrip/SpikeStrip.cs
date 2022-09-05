using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Animation;
using AutomaticRoadblocks.Sound;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Vehicles;
using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.SpikeStrip
{
    /// <summary>
    /// A basic spike strip implementation which implements the <see cref="ISpikeStrip"/>.
    /// </summary>
    public class SpikeStrip : ISpikeStrip
    {
        protected static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        protected static readonly IGame Game = IoC.Instance.GetInstance<IGame>();

        private AnimationExecutor _animation;

        public SpikeStrip(Road road, ESpikeStripLocation location)
        {
            Assert.NotNull(road, "road cannot be null");
            Road = road;
            Location = location;
        }

        #region Properties

        /// <inheritdoc />
        public Vector3 Position
        {
            get
            {
                return Location switch
                {
                    ESpikeStripLocation.Left => Road.LeftSide,
                    ESpikeStripLocation.Middle => Road.Position,
                    _ => Road.RightSide
                };
            }
        }

        /// <inheritdoc />
        public float Heading
        {
            get
            {
                return Location switch
                {
                    ESpikeStripLocation.Left => Road.Node.Heading + 90,
                    _ => Road.Node.Heading - 90
                };
            }
        }

        /// <inheritdoc />
        public ESpikeStripLocation Location { get; }

        /// <inheritdoc />
        public ESpikeStripState State { get; private set; } = ESpikeStripState.Preparing;

        /// <inheritdoc />
        public event SpikeStripEvents.SpikeStripStateChanged StateChanged;

        /// <summary>
        /// The road this spike strip is deployed on.
        /// </summary>
        private Road Road { get; }

        /// <summary>
        /// The spike strip instance.
        /// </summary>
        private Object Instance { get; set; }

        /// <summary>
        /// Verify if the instance has been invalidated.
        /// </summary>
        private bool IsInvalid => Instance == null || !Instance.IsValid();

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => Road.IsPreviewActive;

        /// <inheritdoc />
        public void CreatePreview()
        {
            if (IsPreviewActive)
                return;

            Game.NewSafeFiber(() =>
            {
                Road.CreatePreview();
                DoInternalSpawn();

                if (!IsInvalid)
                    PreviewUtils.TransformToPreview(Instance);

                while (IsPreviewActive)
                {
                    Game.DrawSphere(Position, 0.2f, Color.Red);
                    Game.FiberYield();
                }
            }, "SpikeStrip.Preview");
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            if (!IsPreviewActive)
                return;

            Road.DeletePreview();
            DoInternalCleanup();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Spawn()
        {
            Game.NewSafeFiber(DoInternalSpawn, "SpikeStrip.Spawn");
        }

        /// <inheritdoc />
        public void Deploy()
        {
            Game.NewSafeFiber(DoInternalDeploy, "SpikeStrip.Deploy");
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
            StateChanged?.Invoke(this, state);
        }

        private void DoInternalSpawn()
        {
            if (!IsInvalid)
                return;

            Logger.Trace($"Spawning spike strip {this}");
            Instance = PropUtils.CreateSpikeStrip(Position, Heading);
            _animation = AnimationHelper.PlayAnimation(Instance, Animations.Dictionaries.StingerDictionary, Animations.SpikeStripIdleUndeployed,
                AnimationFlags.StayInEndFrame);
            UpdateState(ESpikeStripState.Undeployed);
        }

        private void DoInternalDeploy()
        {
            if (IsInvalid)
                DoInternalSpawn();

            // wait for the spike strip to become available
            while (State != ESpikeStripState.Undeployed)
            {
                Game.FiberYield();
            }

            Logger.Trace($"Deploying spike strip {this}");
            UpdateState(ESpikeStripState.Deploying);
            StopCurrentAnimation();
            DoDeployAnimation();
            UpdateState(ESpikeStripState.Deployed);
            StartMonitor();
        }

        private void DoInternalCleanup()
        {
            if (Instance == null)
                return;

            UpdateState(ESpikeStripState.Disposed);
            EntityUtils.Remove(Instance);
        }

        private void StartMonitor()
        {
            Game.NewSafeFiber(() =>
            {
                while (State is not ESpikeStripState.Preparing or ESpikeStripState.Undeployed or ESpikeStripState.Disposed)
                {
                    DoNearbyVehiclesCheck();
                    DoAdditionalVerifications();
                    Game.FiberYield();
                }
            }, "SpikeStrip.Monitor");
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
            var instancePosition = Instance.Position;
            var orientation = Instance.Orientation;
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

            SoundHelper.PlaySound(Instance, Sounds.StingerDrop, Sounds.StingerDropRef);
            _animation = AnimationHelper.PlayAnimation(Instance, Animations.Dictionaries.StingerDictionary, Animations.SpikeStripDeploy,
                AnimationFlags.StayInEndFrame);
            _animation.WaitForCompletion();
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

        #endregion
    }
}