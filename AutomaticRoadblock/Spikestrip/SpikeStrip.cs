using System.Collections.Generic;
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
    public class SpikeStrip : ISpikeStrip
    {
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        private static readonly IGame Game = IoC.Instance.GetInstance<IGame>();

        private bool _deployed;
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
            _deployed = false;
            if (Instance == null)
                return;

            EntityUtils.Remove(Instance);
            UpdateState(ESpikeStripState.Disposed);
        }

        #endregion

        #region Functions

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
            StartMonitor();
            UpdateState(ESpikeStripState.Deployed);
        }

        private void StartMonitor()
        {
            _deployed = true;
            Game.NewSafeFiber(() =>
            {
                while (_deployed)
                {
                    foreach (var wheel in from vehicle in NearbyVehicles()
                             from wheelIndex in AllWheels()
                             where vehicle.HasBone(wheelIndex.BoneName) && !IsVehicleTireBurst(vehicle, wheelIndex, false)
                             select vehicle.Wheels[wheelIndex.Index]
                             into wheel
                             where IsTouchingTheInstance(wheel.LastContactPoint)
                             select wheel)
                    {
                        wheel.BurstTire();
                    }

                    Game.FiberYield();
                }
            }, "SpikeStrip.Monitor");
        }

        private IEnumerable<Vehicle> NearbyVehicles()
        {
            return World
                .GetEntities(Position, PropUtils.Models.SpikeStrip.Dimensions.Y, GetEntitiesFlags.ConsiderGroundVehicles)
                .OfType<Vehicle>()
                .Where(x => x.IsValid());
        }

        private void UpdateState(ESpikeStripState state)
        {
            State = state;
            StateChanged?.Invoke(this, state);
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