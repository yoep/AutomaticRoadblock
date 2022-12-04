using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const float DeploySpikeStripRange = 50f;
        private const float PlacementInFrontOfVehicle = 0.1f;
        private const int DelayBetweenStateChangeAndUndeploy = 2 * 1000;
        private const int DelayUndeployment = 1500;

        private bool _hasBeenDeployed;
        private long _spikeStripDeployStartedAt;

        public SpikeStripSlot(ISpikeStripDispatcher spikeStripDispatcher, Road street, Road.Lane lane, Vehicle targetVehicle, float heading,
            bool shouldAddLights, float offset = 0)
            : base(lane, BarrierModel.None, BarrierModel.None, EBackupUnit.LocalPatrol, heading, shouldAddLights, false, offset)
        {
            Assert.NotNull(spikeStripDispatcher, "spikeStripDispatcher cannot be null");
            Assert.NotNull(targetVehicle, "targetVehicle cannot be null");
            SpikeStripDispatcher = spikeStripDispatcher;
            TargetVehicle = targetVehicle;
            Road = street;
            Location = DetermineLocation();
            NumberOfCops = 1;

            Initialize();
        }

        #region Properties

        /// <inheritdoc />
        public override IList<ARPed> CopsJoiningThePursuit => new List<ARPed>();

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
            .Select(x => (ARSpikeStrip)x)
            .Where(x => x is { SpikeStrip: { } })
            .Select(x => x.SpikeStrip)
            .FirstOrDefault();

        /// <summary>
        /// Determine the spike strip location.
        /// </summary>
        private ESpikeStripLocation Location { get; }

        #endregion

        #region Methods

        public override void Spawn()
        {
            base.Spawn();
            StartMonitor();
        }

        /// <inheritdoc />
        public override void Release(bool releaseAll = false)
        {
            SpikeStrip?.Undeploy();
        }

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override void InitializeScenery()
        {
            // create the spike strip
            Instances.Add(new ARSpikeStrip(CreateSpikeStripInstance()));
        }

        /// <inheritdoc />
        protected override void InitializeLights()
        {
            // no-op
        }

        /// <inheritdoc />
        protected override Vector3 CalculatePositionBehindVehicle()
        {
            return CalculateVehiclePositionOnSide() + CalculateDirectionInFrontOfVehicle(PlacementInFrontOfVehicle);
        }

        /// <inheritdoc />
        protected override float CalculateCopHeading()
        {
            return Location switch
            {
                ESpikeStripLocation.Right => Heading + 90,
                _ => Heading - 90
            };
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
            var offset = VehicleLength + PlacementInFrontOfVehicle;
            var spikeStrip = SpikeStripDispatcher.Spawn(Road, Lane, Location, TargetVehicle, offset);
            spikeStrip.StateChanged += SpikeStripStateChanged;
            return spikeStrip;
        }

        private void SpikeStripStateChanged(ISpikeStrip spikeStrip, ESpikeStripState newState)
        {
            switch (newState)
            {
                case ESpikeStripState.Hit:
                    OnSpikeStripHit();
                    break;
                case ESpikeStripState.Bypassed:
                    OnSpikeStripBypassed();
                    break;
                case ESpikeStripState.Deployed:
                    OnSpikeStripDeployed();
                    break;
                case ESpikeStripState.Undeployed:
                    OnSpikeStripUndeployed();
                    break;
            }
        }

        [Conditional("DEBUG")]
        private void OnSpikeStripDeployed()
        {
            var timeTaken = (DateTime.Now.Ticks - _spikeStripDeployStartedAt) / TimeSpan.TicksPerMillisecond;
           var distanceFromTarget = TargetVehicle.DistanceTo2D(SpikeStrip.Position);
            var deployTimeColor = timeTaken < 1000 ? "g" : "r";
            var distanceColor = distanceFromTarget >= 15f ? "g" : "r";

            Game.DisplayNotificationDebug("~b~Spike strip deployed~s~~n~" +
                                          $"Deploy time: ~{deployTimeColor}~{timeTaken}ms~s~~n~" +
                                          $"Distance: ~{distanceColor}~{distanceFromTarget}");
            Logger.Trace("--- Spike strip slot performance stats ---\n" +
                         $"Deploy time: ~{deployTimeColor}~{timeTaken}ms\n" +
                         $"Distance: ~{distanceColor}~{distanceFromTarget}");
        }

        [Conditional("DEBUG")]
        private void OnSpikeStripUndeployed()
        {
            Game.DisplayNotificationDebug("~p~Spike strip undeployed");
        }

        private void OnSpikeStripBypassed()
        {
            DoUndeploy();
        }

        private void OnSpikeStripHit()
        {
            // delay the undeploy to pop additional tires
            Game.NewSafeFiber(() =>
            {
                GameFiber.Wait(DelayUndeployment);
                DoUndeploy();
            }, "SpikeStripStateChanged.Hit");
        }

        private void StartMonitor()
        {
            if (_hasBeenDeployed)
                return;

            Logger.Trace("Starting spike strip slot monitor");
            Game.NewSafeFiber(() =>
            {
                while (!_hasBeenDeployed && TargetVehicle != null && TargetVehicle.IsValid())
                {
                    if (TargetVehicle.DistanceTo(Position) <= DeploySpikeStripRange)
                    {
                        DoSpikeStripDeploy();
                    }

                    Game.FiberYield();
                }

                Logger.Debug("Spike strip monitor stopped");
            }, $"{GetType()}.Monitor");
        }

        private void DoUndeploy()
        {
            Game.NewSafeFiber(() =>
            {
                GameFiber.Wait(DelayBetweenStateChangeAndUndeploy);
                ExecuteWithCop(cop =>
                    AnimationHelper.PlayAnimation(cop.GameInstance, Animations.Dictionaries.ObjectDictionary, Animations.ObjectPickup, AnimationFlags.None));
                SpikeStrip?.Undeploy();
            }, "SpikeStripSlot.Undeploy");
        }

        private void DoSpikeStripDeploy()
        {
            if (_hasBeenDeployed)
            {
                Game.DisplayNotificationDebug($"~r~Unable to deploy {GetType()}, has already been deployed");
                return;
            }

            Logger.Trace($"Starting deployment of spike strip as target is in range ({TargetVehicle.DistanceTo(Position)}), deploying spike strip");
            var spikeStrip = SpikeStrip;
            _spikeStripDeployStartedAt = DateTime.Now.Ticks;
            ExecuteWithCop(cop =>
                AnimationHelper.PlayAnimation(cop.GameInstance, Animations.Dictionaries.GrenadeDictionary, Animations.ThrowShortLow, AnimationFlags.None));
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
            return MathHelper.ConvertHeadingToDirection(Heading) * (VehicleLength + additionalVerticalOffset);
        }

        private ESpikeStripLocation DetermineLocation()
        {
            var distanceLeft = Road.LeftSide.DistanceTo2D(Lane.Position);
            var distanceMiddle = Road.Position.DistanceTo2D(Lane.Position);
            var distanceRight = Road.RightSide.DistanceTo2D(Lane.Position);
            var totalLanes = Road.NumberOfLanesSameDirection + Road.NumberOfLanesOppositeDirection;

            Logger.Trace($"Spike strip location data, {nameof(Lane.IsOppositeHeadingOfRoadNodeHeading)}: {Lane.IsOppositeHeadingOfRoadNodeHeading}, " +
                         $"{nameof(distanceMiddle)}: {distanceMiddle},  {nameof(distanceLeft)}: {distanceLeft},  {nameof(distanceRight)}: {distanceRight}");
            if (totalLanes > 2 && distanceMiddle < distanceRight && distanceMiddle < distanceLeft)
            {
                return ESpikeStripLocation.Middle;
            }

            return distanceRight <= distanceLeft ? ESpikeStripLocation.Right : ESpikeStripLocation.Left;
        }

        private void ExecuteWithCop(Action<ARPed> action)
        {
            Game.NewSafeFiber(() =>
            {
                var cop = Cops.FirstOrDefault();
                if (cop != null)
                {
                    Logger.Trace($"Playing throw animation for spike strip {this}");
                    action.Invoke(cop);
                }
                else
                {
                    Logger.Warn($"Spike strip slot has no valid cop ped, {this}");
                }
            }, $"{GetType()}.ExecuteWithCop");
        }

        #endregion
    }
}