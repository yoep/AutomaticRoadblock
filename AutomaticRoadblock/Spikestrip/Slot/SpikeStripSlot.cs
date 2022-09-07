using System.Linq;
using AutomaticRoadblocks.Animation;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.SpikeStrip.Dispatcher;
using AutomaticRoadblocks.Utils.Road;
using JetBrains.Annotations;
using Rage;
using VehicleType = AutomaticRoadblocks.Vehicles.VehicleType;

namespace AutomaticRoadblocks.SpikeStrip.Slot
{
    /// <summary>
    /// A <see cref="IRoadblockSlot"/> which deploys a spike strip.
    /// This slot won't create any barriers and will always use the local vehicle type.
    /// </summary>
    public class SpikeStripSlot : AbstractRoadblockSlot
    {
        private const float DeploySpikeStripRange = 40f;
        private const int DelayBetweenStateChangeAndUndeploy = 2 * 1000;

        private bool _hasBeenDeployed;

        public SpikeStripSlot(ISpikeStripDispatcher spikeStripDispatcher, Road road, Road.Lane lane, Vehicle targetVehicle, float heading, bool shouldAddLights,
            float offset = 0)
            : base(lane, BarrierType.None, VehicleType.Local, heading, shouldAddLights, false, offset)
        {
            Assert.NotNull(spikeStripDispatcher, "spikeStripDispatcher cannot be null");
            Assert.NotNull(targetVehicle, "targetVehicle cannot be null");
            SpikeStripDispatcher = spikeStripDispatcher;
            TargetVehicle = targetVehicle;
            Road = road;

            Initialize();
        }

        #region Properties

        /// <summary>
        /// The road this spike strip slot is placed on.
        /// </summary>
        private Road Road { get; }

        /// <summary>
        /// The target vehicle of the spike strip.
        /// </summary>
        private Vehicle TargetVehicle { get; }

        /// <summary>
        /// The spike strip dispatcher to use for creating an instance.
        /// </summary>
        private ISpikeStripDispatcher SpikeStripDispatcher { get; }

        /// <summary>
        /// Retrieve the spike strip instance of this slot.
        /// </summary>
        [CanBeNull]
        private ISpikeStrip SpikeStrip => Instances
            .Where(x => x.Type == EEntityType.SpikeStrip)
            .Select(x => (ARSpikeStrip)x.Instance)
            .Where(x => x.SpikeStrip != null)
            .Select(x => x.SpikeStrip)
            .FirstOrDefault();

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Release()
        {
            SpikeStrip?.Undeploy();
        }

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override void InitializeCops()
        {
            Instances.Add(new InstanceSlot(EEntityType.CopPed, CalculateCopOrVehiclePosition(), Heading - 90, PedFactory.CreateLocaleCop));
        }

        /// <inheritdoc />
        protected override void InitializeScenery()
        {
            // create the spike strip
            Instances.Add(new InstanceSlot(EEntityType.SpikeStrip, Position, Heading,
                (_, _) => new ARSpikeStrip(CreateSpikeStripInstance())));
        }

        /// <inheritdoc />
        protected override void InitializeLights()
        {
            // no-op
        }

        /// <inheritdoc />
        protected override float CalculateVehicleHeading()
        {
            return Heading + Random.Next(-VehicleHeadingMaxOffset, VehicleHeadingMaxOffset);
        }

        /// <inheritdoc />
        protected override Vector3 CalculateVehiclePosition()
        {
            return OffsetPosition
                   + CalculateCopOrVehiclePosition()
                   + MathHelper.ConvertHeadingToDirection(Heading) * VehicleModel.Dimensions.Y;
        }

        private ESpikeStripLocation DetermineLocation()
        {
            var distanceLeft = Road.LeftSide.DistanceTo2D(Lane.Position);
            var distanceMiddle = Road.Position.DistanceTo2D(Lane.Position);
            var distanceRight = Road.RightSide.DistanceTo2D(Lane.Position);

            if (distanceLeft < distanceMiddle && distanceLeft < distanceRight)
            {
                return ESpikeStripLocation.Left;
            }

            if (distanceRight < distanceMiddle && distanceRight < distanceLeft)
            {
                return ESpikeStripLocation.Right;
            }

            return ESpikeStripLocation.Middle;
        }

        private ISpikeStrip CreateSpikeStripInstance()
        {
            var spikeStrip = SpikeStripDispatcher.Spawn(Road, Lane, DetermineLocation(), TargetVehicle);
            spikeStrip.StateChanged += SpikeStripStateChanged;
            return spikeStrip;
        }

        private void SpikeStripStateChanged(ISpikeStrip spikeStrip, ESpikeStripState newState)
        {
            switch (newState)
            {
                case ESpikeStripState.Undeployed:
                    StartMonitor();
                    break;
                case ESpikeStripState.Hit:
                case ESpikeStripState.Bypassed:
                    DoUndeploy();
                    break;
            }
        }

        private void StartMonitor()
        {
            if (_hasBeenDeployed)
                return;

            Logger.Trace("Starting spike strip slot monitor");
            _hasBeenDeployed = true;
            Game.NewSafeFiber(() =>
            {
                var spikeStrip = SpikeStrip;
                while (spikeStrip?.State == ESpikeStripState.Undeployed)
                {
                    var distanceToSlot = TargetVehicle.DistanceTo2D(Position);
                    if (distanceToSlot <= DeploySpikeStripRange)
                    {
                        DoSpikeStripDeploy(distanceToSlot);
                    }

                    Game.FiberYield();
                }
            }, "SpikeStripSlot.Monitor");
        }

        private void DoUndeploy()
        {
            Game.NewSafeFiber(() =>
            {
                GameFiber.Wait(DelayBetweenStateChangeAndUndeploy);
                var cop = Cops.First();
                AnimationHelper.PlayAnimation(cop.GameInstance, Animations.Dictionaries.ObjectDictionary, Animations.ObjectPickup, AnimationFlags.None);
                SpikeStrip?.Undeploy();
            }, "SpikeStripSlot.Undeploy");
        }

        private void DoSpikeStripDeploy(float distanceToSlot)
        {
            var spikeStrip = SpikeStrip;
            Logger.Trace($"Target vehicle is in range of spike strip ({distanceToSlot}), deploying spike strip {spikeStrip}");
            var cop = Cops.First();
            AnimationHelper.PlayAnimation(cop.GameInstance, Animations.Dictionaries.GrenadeDictionary, Animations.ThrowShortLow, AnimationFlags.None);
            spikeStrip?.Deploy();
        }

        private Vector3 CalculateCopOrVehiclePosition()
        {
            return Position + MathHelper.ConvertHeadingToDirection(Heading + 90) * (Lane.Width / 2);
        }

        #endregion
    }
}