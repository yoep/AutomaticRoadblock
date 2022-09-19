using System.Linq;
using AutomaticRoadblocks.Animation;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.SpikeStrip.Dispatcher;
using AutomaticRoadblocks.Street.Info;
using JetBrains.Annotations;
using Rage;

namespace AutomaticRoadblocks.SpikeStrip.Slot
{
    /// <summary>
    /// A <see cref="IRoadblockSlot"/> which deploys a spike strip.
    /// This slot won't create any barriers and will always use the local vehicle type.
    /// </summary>
    public class SpikeStripSlot : AbstractRoadblockSlot
    {
        private const float DeploySpikeStripRange = 55f;
        private const int DelayBetweenStateChangeAndUndeploy = 2 * 1000;
        private const float PlacementInFrontOfVehicle = 0.1f;

        private bool _hasBeenDeployed;

        public SpikeStripSlot(ISpikeStripDispatcher spikeStripDispatcher, Road street, Road.Lane lane, Vehicle targetVehicle, float heading,
            bool shouldAddLights,
            float offset = 0)
            : base(lane, BarrierModel.None, BarrierModel.None, EBackupUnit.LocalPatrol, heading, shouldAddLights, false, offset)
        {
            Assert.NotNull(spikeStripDispatcher, "spikeStripDispatcher cannot be null");
            Assert.NotNull(targetVehicle, "targetVehicle cannot be null");
            SpikeStripDispatcher = spikeStripDispatcher;
            TargetVehicle = targetVehicle;
            Road = street;
            Location = DetermineLocation();

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

        /// <summary>
        /// Determine the spike strip location.
        /// </summary>
        private ESpikeStripLocation Location { get; }

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
            var copPosition = CalculateVehiclePositionOnSide() + CalculateDirectionInFrontOfVehicle(PlacementInFrontOfVehicle);
            var copHeading = Location switch
            {
                ESpikeStripLocation.Right => Heading + 90,
                _ => Heading - 90
            };

            Instances.Add(new InstanceSlot(EEntityType.CopPed, copPosition, copHeading,
                (position, heading) => PedFactory.CreateBasicCopWeapons(PedFactory.CreateLocaleCop(position, heading))));
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
            return CalculateVehiclePositionOnSide();
        }

        private ISpikeStrip CreateSpikeStripInstance()
        {
            var offset = VehicleModel.Dimensions.Y + PlacementInFrontOfVehicle;
            var spikeStrip = SpikeStripDispatcher.Spawn(Road, Lane, Location, TargetVehicle, offset);
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
            Game.NewSafeFiber(() =>
            {
                var spikeStrip = SpikeStrip;
                while (spikeStrip?.State == ESpikeStripState.Undeployed)
                {
                    if (TargetVehicle.DistanceTo2D(Position) <= DeploySpikeStripRange)
                    {
                        DoSpikeStripDeploy();
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

        private void DoSpikeStripDeploy()
        {
            if (_hasBeenDeployed)
                return;

            Logger.Trace($"Target vehicle is in range of spike strip ({TargetVehicle.DistanceTo2D(Position)}), deploying spike strip");
            var spikeStrip = SpikeStrip;
            var cop = Cops.First();
            AnimationHelper.PlayAnimation(cop.GameInstance, Animations.Dictionaries.GrenadeDictionary, Animations.ThrowShortLow, AnimationFlags.None);
            spikeStrip?.Deploy();
            _hasBeenDeployed = true;
        }

        private Vector3 CalculateVehiclePositionOnSide()
        {
            var position = OffsetPosition;
            var headingRotation = Location switch
            {
                ESpikeStripLocation.Right => Heading - 90f,
                _ => Heading + 90f
            };

            if (Location == ESpikeStripLocation.Middle)
            {
                position += MathHelper.ConvertHeadingToDirection(Heading) * 2f;
            }

            return position + MathHelper.ConvertHeadingToDirection(headingRotation) * (Lane.Width / 2);
        }

        private Vector3 CalculateDirectionInFrontOfVehicle(float additionalVerticalOffset)
        {
            return MathHelper.ConvertHeadingToDirection(Heading) * (VehicleModel.Dimensions.Y + additionalVerticalOffset);
        }

        private ESpikeStripLocation DetermineLocation()
        {
            var distanceLeft = Road.LeftSide.DistanceTo2D(Lane.Position);
            var distanceMiddle = Road.Position.DistanceTo2D(Lane.Position);
            var distanceRight = Road.RightSide.DistanceTo2D(Lane.Position);
            var totalLanes = Road.NumberOfLanesSameDirection + Road.NumberOfLanesOppositeDirection;

            if (totalLanes > 2 && distanceMiddle < distanceRight && distanceMiddle < distanceLeft)
            {
                return ESpikeStripLocation.Middle;
            }

            if (!Lane.IsOppositeHeadingOfRoadNodeHeading)
            {
                return distanceRight <= distanceLeft ? ESpikeStripLocation.Right : ESpikeStripLocation.Left;
            }

            return distanceRight <= distanceLeft ? ESpikeStripLocation.Left : ESpikeStripLocation.Right;
        }

        #endregion
    }
}