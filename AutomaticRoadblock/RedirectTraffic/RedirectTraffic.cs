using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Vehicles;
using Rage;
using VehicleType = AutomaticRoadblocks.Vehicles.VehicleType;

namespace AutomaticRoadblocks.RedirectTraffic
{
    public class RedirectTraffic : IPlaceableInstance
    {
        private const float DefaultVehicleWidth = 2f;
        private const float DefaultVehicleLength = 4f;
        private const string RedirectTrafficAnimation = "amb@world_human_car_park_attendant@male@base";

        private static readonly IGame Game = IoC.Instance.GetInstance<IGame>();
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        private readonly List<InstanceSlot> _instances = new();

        private Blip _blip;

        public RedirectTraffic(Request request)
        {
            Assert.NotNull(request.Road, "road cannot be null");
            Assert.NotNull(request.VehicleType, "vehicleType cannot be null");
            Assert.NotNull(request.ConeType, "coneType cannot be null");
            Assert.NotNull(request.Type, "type cannot be null");
            Road = request.Road;
            Lane = GetLaneClosestToPlayer();
            VehicleType = request.VehicleType;
            ConeType = request.ConeType;
            Type = request.Type;
            ConeDistance = request.ConeDistance;
            EnableRedirectionArrow = request.EnableRedirectionArrow;
            EnableLights = request.EnableLights;
            Offset = request.Offset;

            Init();
        }

        #region Properties

        /// <summary>
        /// The position of the redirect traffic instance.
        /// </summary>
        public Vector3 Position => PositionBasedOnType();

        /// <summary>
        /// The offset position in regards to the node of the redirect traffic instance.
        /// </summary>
        public Vector3 OffsetPosition => Position + MathHelper.ConvertHeadingToDirection(Lane.Heading) * Offset;

        /// <summary>
        /// The road on which this redirect traffic instance is created.
        /// </summary>
        public Road Road { get; }

        /// <summary>
        /// The lane closest to the player which is used by the redirect traffic instance.
        /// </summary>
        public Road.Lane Lane { get; }

        /// <summary>
        /// The vehicle type of the redirect traffic instance.
        /// </summary>
        public VehicleType VehicleType { get; }

        /// <summary>
        /// The cone type of the redirect traffic instance.
        /// </summary>
        public BarrierType ConeType { get; }

        /// <summary>
        /// The type of the redirect traffic instance.
        /// </summary>
        public RedirectTrafficType Type { get; }

        /// <summary>
        /// The distance along the road the cones should be placed.
        /// </summary>
        public float ConeDistance { get; }

        /// <summary>
        /// The indication if the redirection arrow is enabled.
        /// </summary>
        public bool EnableRedirectionArrow { get; }

        /// <summary>
        /// The indication if lights are enabled for this redirect traffic instance.
        /// </summary>
        public bool EnableLights { get; }

        /// <summary>
        /// Get the relative offset for the position in regards to the vehicle node.
        /// </summary>
        public float Offset { get; }

        /// <summary>
        /// The vehicle model to use for the vehicle within this instance.
        /// </summary>
        private Model VehicleModel { get; set; }

        /// <summary>
        /// Check if the current traffic redirection is on the most left lane of the road
        /// (for the lanes heading in the same direction as <see cref="Lane"/>).
        /// </summary>
        private bool IsLeftSideOfLanes => IsLeftSideOfLanesInTheSameHeadingAsTheSelectedLane();

        /// <summary>
        /// The cop instance of this redirect traffic instance.
        /// </summary>
        private ARPed Cop => _instances
            .Where(x => x.Type == EntityType.CopPed)
            .Select(x => x.Instance)
            .Select(x => (ARPed)x)
            .First();

        /// <summary>
        /// The vehicle instance of this redirect traffic instance.
        /// </summary>
        private ARVehicle Vehicle => _instances
            .Where(x => x.Type == EntityType.CopVehicle)
            .Select(x => x.Instance)
            .Select(x => (ARVehicle)x)
            .First();

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => _instances.Any(x => x.IsPreviewActive);

        /// <inheritdoc />
        public void CreatePreview()
        {
            _instances.ForEach(x => x.CreatePreview());
            Road.CreatePreview();
            CreateBlip();
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            _instances.ForEach(x => x.DeletePreview());
            Road.DeletePreview();
            DeleteBlip();
        }

        #endregion

        #region IRedirectTraffic

        /// <inheritdoc />
        public bool Spawn()
        {
            CreateBlip();
            var result = _instances.All(x => x.Spawn());

            Vehicle.GameInstance.IndicatorLightsStatus = VehicleIndicatorLightsStatus.Both;
            Cop.Attach(PropUtils.CreateWand(), PedBoneId.RightPhHand);
            AnimationUtils.PlayAnimation(Cop.GameInstance, RedirectTrafficAnimation, "base", AnimationFlags.Loop);
            return result;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return
                $"{nameof(Position)}: {Position}, {nameof(OffsetPosition)}: {OffsetPosition}, {nameof(Type)}: {Type}, {nameof(VehicleType)}: {VehicleType}, " +
                $"{nameof(ConeType)}: {ConeType}, {nameof(IsLeftSideOfLanes)}: {IsLeftSideOfLanes},\n" +
                $"{nameof(Road)}: {Road}\n" +
                $"Using {nameof(Lane)}: {Lane}";
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            Cop.DeleteAttachments();
            _instances.ForEach(x => x.Dispose());
            DeleteBlip();
        }

        #endregion

        #region Functions

        private void Init()
        {
            InitializeVehicle();
            InitializeCop();
            InitializeScenery();
        }

        private void InitializeVehicle()
        {
            if (VehicleType == VehicleType.None)
                return;

            var rotation = IsLeftSideOfLanes ? -35 : 35;
            VehicleModel = VehicleFactory.CreateModel(VehicleType, OffsetPosition);

            _instances.Add(new InstanceSlot(EntityType.CopVehicle, OffsetPosition, Lane.Heading + rotation,
                (position, heading) => VehicleFactory.CreateWithModel(VehicleModel, position, heading)));
        }

        private void InitializeCop()
        {
            var distanceBehindVehicle = GetVehicleLength() - 1f;
            var copPedHeading = Lane.Heading - 180;
            var positionBehindVehicle = OffsetPosition + MathHelper.ConvertHeadingToDirection(copPedHeading) * distanceBehindVehicle;

            _instances.Add(new InstanceSlot(EntityType.CopPed, positionBehindVehicle, copPedHeading,
                (position, heading) => CreateCop(position, heading)));
        }

        private void InitializeScenery()
        {
            PlaceConesAlongTheRoad();
            var coneEndPosition = PlaceConesBehindTheVehicle();
            PlaceVehiclesStoppedSign(coneEndPosition);

            if (EnableRedirectionArrow)
                PlaceRedirectionArrow(coneEndPosition);
            if (EnableLights)
                InitializeVehicleStoppedLight(coneEndPosition);
        }

        private ARPed CreateCop(Vector3 position, float heading)
        {
            if (VehicleType != VehicleType.None)
                return PedFactory.CreateCopForVehicle(VehicleModel, position, heading);

            return PedFactory.CreateLocaleCop(position, heading);
        }

        private void PlaceConesAlongTheRoad()
        {
            var placementDirection = MathHelper.ConvertHeadingToDirection(Lane.Heading);
            var startPosition = OffsetPosition + ConeStartDirection();
            var actualConeLength = ConeType.Width + ConeType.Spacing;
            var coneDistance = ConeDistance + GetVehicleLength();
            var totalCones = coneDistance / actualConeLength;

            Logger.Trace(
                $"Creating a total of {totalCones} cones along the road with type {ConeType} for a length of {coneDistance} (ConeTypeWidth: {ConeType.Width}, ConeTypeSpacing: {ConeType.Spacing})");
            for (var i = 0; i < totalCones; i++)
            {
                _instances.Add(new InstanceSlot(EntityType.Scenery, startPosition, ConeHeading(),
                    (position, heading) => BarrierFactory.Create(ConeType, position, heading)));
                startPosition += placementDirection * actualConeLength;
            }
        }

        private Vector3 PlaceConesBehindTheVehicle()
        {
            var coneDistance = ConeType.Width + ConeType.Spacing;
            var totalCones = (int)Math.Floor(Lane.Width / coneDistance);
            var placementDirectionSide = IsLeftSideOfLanes ? 90 : -90;
            var placementDirection = MathHelper.ConvertHeadingToDirection(Lane.Heading - 180) * coneDistance +
                                     MathHelper.ConvertHeadingToDirection(Lane.Heading + placementDirectionSide) * coneDistance;
            var startPosition = OffsetPosition + ConeStartDirection(0.5f);

            Logger.Trace($"Creating a total of {totalCones} cones behind the vehicle for a lane width of {Lane.Width}");
            for (var i = 0; i < totalCones; i++)
            {
                _instances.Add(new InstanceSlot(EntityType.Scenery, startPosition, ConeHeading(),
                    (position, heading) => BarrierFactory.Create(ConeType, position, heading)));
                startPosition += placementDirection * coneDistance;
            }

            return startPosition;
        }

        private void PlaceVehiclesStoppedSign(Vector3 coneEndPosition)
        {
            var signPosition = VehicleStoppedSignPosition(coneEndPosition);

            _instances.Add(new InstanceSlot(EntityType.Scenery, signPosition, Lane.Heading,
                (position, heading) => new ARScenery(PropUtils.StoppedVehiclesSign(position, heading))));
        }

        private void PlaceRedirectionArrow(Vector3 coneEndPosition)
        {
            var sideDirection = SignSideDirection();
            var signPosition = coneEndPosition
                               + MathHelper.ConvertHeadingToDirection(sideDirection) * 1.5f
                               + MathHelper.ConvertHeadingToDirection(Lane.Heading - 180) * 1f;

            _instances.Add(new InstanceSlot(EntityType.Scenery, signPosition, Lane.Heading,
                (position, heading) => new ARScenery(IsLeftSideOfLanes
                    ? PropUtils.CreateWorkerBarrierArrowRight(position, heading)
                    : PropUtils.RedirectTrafficArrowLeft(position, heading))));
        }

        private Vector3 VehicleStoppedSignPosition(Vector3 coneEndPosition)
        {
            var sideDirection = SignSideDirection();
            var distanceToTheSide = Lane.Width / (Type == RedirectTrafficType.Lane ? 1.5f : 2.5f);
            var signPosition = coneEndPosition
                               + MathHelper.ConvertHeadingToDirection(sideDirection) * distanceToTheSide
                               + MathHelper.ConvertHeadingToDirection(Lane.Heading - 180) * 6f;
            return signPosition;
        }

        private void InitializeVehicleStoppedLight(Vector3 coneEndPosition)
        {
            var groundLightPosition = VehicleStoppedSignPosition(coneEndPosition) +
                                      MathHelper.ConvertHeadingToDirection(Lane.Heading - 180) * 1.5f;

            _instances.Add(new InstanceSlot(EntityType.Scenery, groundLightPosition, Lane.Heading - 180,
                (position, heading) => new ARScenery(PropUtils.CreateGroundFloodLight(position, heading))));
        }

        private void CreateBlip()
        {
            if (_blip != null)
                return;

            Logger.Trace("Creating roadblock blip");
            _blip = new Blip(OffsetPosition)
            {
                IsRouteEnabled = false,
                IsFriendly = true,
                Scale = 1f,
                Color = Color.LightBlue
            };
        }

        private void DeleteBlip()
        {
            if (_blip == null)
                return;

            _blip.Delete();
            _blip = null;
        }

        private Road.Lane GetLaneClosestToPlayer()
        {
            var playerPosition = Game.PlayerPosition;
            var rightSide = Road.RightSide;
            var leftSide = Road.LeftSide;
            var closestLaneDistance = 9999f;
            var closestLane = (Road.Lane)null;
            var closestTo = rightSide;

            if (leftSide.DistanceTo(playerPosition) < rightSide.DistanceTo(playerPosition))
            {
                Logger.Debug("Using left side of the road for redirecting the traffic");
                closestTo = leftSide;
            }
            else
            {
                Logger.Debug("Using right side of the road for redirecting the traffic");
            }

            foreach (var lane in Road.Lanes)
            {
                var distanceToPlayer = lane.Position.DistanceTo(closestTo);

                if (distanceToPlayer > closestLaneDistance)
                    continue;

                closestLaneDistance = distanceToPlayer;
                closestLane = lane;
            }

            return closestLane;
        }

        private int SignSideDirection()
        {
            return IsLeftSideOfLanes ? 90 : -90;
        }

        private Vector3 ConeStartDirection(float additionalDistanceBehindVehicle = 0f)
        {
            var vehicleWidth = GetVehicleWidth() + 0.5f;
            var vehicleLength = GetVehicleLength() - 1f;
            var placementSide = IsLeftSideOfLanes ? -90 : 90;

            if (VehicleType != VehicleType.None)
            {
                vehicleWidth = VehicleModel.Dimensions.X;
                vehicleLength = VehicleModel.Dimensions.Y;
            }
            else
            {
                Logger.Debug("No vehicle selected, using default vehicle values for calculating cone direction");
            }

            return MathHelper.ConvertHeadingToDirection(Lane.Heading + placementSide) * vehicleWidth +
                   MathHelper.ConvertHeadingToDirection(Lane.Heading - 180) * (vehicleLength + additionalDistanceBehindVehicle);
        }

        private float ConeHeading()
        {
            if (ConeType == BarrierType.ConeWithLight)
                return Lane.Heading - 90;

            return Lane.Heading;
        }

        private float GetVehicleWidth()
        {
            return VehicleType == VehicleType.None ? DefaultVehicleWidth : VehicleModel.Dimensions.X;
        }

        private float GetVehicleLength()
        {
            return VehicleType == VehicleType.None ? DefaultVehicleLength : VehicleModel.Dimensions.Y;
        }

        private Vector3 PositionBasedOnType()
        {
            var lanePosition = Lane.Position;
            var shoulderRotation = IsLeftSideOfLanes ? 90 : -90;

            if (Type == RedirectTrafficType.Shoulder)
                lanePosition += MathHelper.ConvertHeadingToDirection(Lane.Heading + shoulderRotation) * (Lane.Width / 2);

            return lanePosition;
        }

        private bool IsLeftSideOfLanesInTheSameHeadingAsTheSelectedLane()
        {
            var distanceToLeftSide = Lane.Position.DistanceTo(Road.LeftSide);
            var distanceToRightSide = Lane.Position.DistanceTo(Road.RightSide);

            Logger.Debug($"Left side closer: {distanceToLeftSide < distanceToRightSide}\n" +
                         $"Right side closer: {distanceToRightSide < distanceToLeftSide}\n" +
                         $"Is lane opposite: {Lane.IsOppositeDirectionOfRoad}");
            var isLeftSideCloser = distanceToLeftSide < distanceToRightSide;

            if (Lane.IsOppositeDirectionOfRoad)
                isLeftSideCloser = !isLeftSideCloser;

            return isLeftSideCloser;
        }

        #endregion

        public class Request
        {
            public Road Road { get; set; }

            public VehicleType VehicleType { get; set; }

            public BarrierType ConeType { get; set; }

            public RedirectTrafficType Type { get; set; }

            public float ConeDistance { get; set; }

            public bool EnableRedirectionArrow { get; set; }

            public bool EnableLights { get; set; }

            public float Offset { get; set; }
        }
    }
}