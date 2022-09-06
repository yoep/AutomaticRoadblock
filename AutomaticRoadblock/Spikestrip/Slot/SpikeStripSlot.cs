using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.SpikeStrip.Dispatcher;
using AutomaticRoadblocks.Utils.Road;
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
        private const float DeploySpikeStripRange = 25f;
        
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
        private ISpikeStrip SpikeStrip => Instances
            .Where(x => x.Type == EEntityType.SpikeStrip)
            .Select(x => (ARSpikeStrip) x.Instance)
            .Select(x => x.SpikeStrip)
            .FirstOrDefault();

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Spawn()
        {
            base.Spawn();
            StartMonitor();
        }

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override void InitializeCops()
        {
            var position = Position + MathHelper.ConvertHeadingToDirection(Heading + 90) * (Lane.Width / 2);
            Instances.Add(new InstanceSlot(EEntityType.CopPed, position, Heading - 90, PedFactory.CreateLocaleCop));
        }

        /// <inheritdoc />
        protected override void InitializeScenery()
        {
            // create the spike strip
            Instances.Add(new InstanceSlot(EEntityType.SpikeStrip, Position, Heading,
                (_, _) => new ARSpikeStrip(SpikeStripDispatcher.Spawn(Road, Lane, DetermineLocation(), TargetVehicle))));
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
            return OffsetPosition + MathHelper.ConvertHeadingToDirection(Heading) * VehicleModel.Dimensions.Y;
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

        private void StartMonitor()
        {
            Game.NewSafeFiber(() =>
            {
                Logger.Trace("Starting spike strip slot monitor");
                var spikeStrip = SpikeStrip;
                
                WaitForTheSpikeStripDeployment(spikeStrip);
                WaitForSpikeStripStateToBeBypassedOrHit(spikeStrip);
                
                spikeStrip.Undeploy();
                Logger.Debug($"Spike strip has been {spikeStrip.State}, undeploying the spike strip");
            }, "SpikeStripSlot.Monitor");
        }

        private void WaitForTheSpikeStripDeployment(ISpikeStrip spikeStrip)
        {
            while (spikeStrip.State is ESpikeStripState.Preparing or ESpikeStripState.Undeployed)
            {
                if (TargetVehicle.DistanceTo2D(Position) <= DeploySpikeStripRange)
                {
                    Logger.Trace($"Target vehicle is in range of spike strip, deploying spike strip {spikeStrip}");
                    spikeStrip.Deploy();
                }

                Game.FiberYield();
            }
        }

        private void WaitForSpikeStripStateToBeBypassedOrHit(ISpikeStrip spikeStrip)
        {
            while (spikeStrip.State is ESpikeStripState.Deploying or ESpikeStripState.Deployed)
            {
                Game.FiberYield();
            }
        }

        #endregion
    }
}