using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Animation;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.RedirectTraffic
{
    public class RedirectTraffic : IPlaceableInstance
    {
        private const float DefaultVehicleWidth = 2f;
        private const float DefaultVehicleLength = 4f;
        private const string RedirectTrafficAnimation = "amb@world_human_car_park_attendant@male@base";
        private const string ConeWithLightName = "cone_with_light";

        private static readonly IGame Game = IoC.Instance.GetInstance<IGame>();
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        private readonly List<IARInstance<Entity>> _instances = new();

        private Blip _blip;

        public RedirectTraffic(Request request)
        {
            Assert.NotNull(request.Road, "road cannot be null");
            Assert.NotNull(request.BackupType, "backupType cannot be null");
            Assert.NotNull(request.ConeType, "coneType cannot be null");
            Assert.NotNull(request.Type, "type cannot be null");
            Road = request.Road;
            Lane = GetLaneClosestToPlayer();
            BackupType = request.BackupType;
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
        /// The backup unit type of the redirect traffic instance.
        /// </summary>
        public EBackupUnit BackupType { get; }

        /// <summary>
        /// The cone type of the redirect traffic instance.
        /// </summary>
        public BarrierModel ConeType { get; }

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
        /// Check if the current traffic redirection is on the most left lane of the road
        /// (for the lanes heading in the same direction as <see cref="Lane"/>).
        /// </summary>
        private bool IsLeftSideOfLanes => IsLeftSideOfLanesInTheSameHeadingAsTheSelectedLane();

        /// <summary>
        /// The cop instance of this redirect traffic instance.
        /// </summary>
        private ARPed Cop => _instances
            .Where(x => x.Type == EEntityType.CopPed)
            .Select(x => (ARPed)x)
            .First();

        /// <summary>
        /// The vehicle instance of this redirect traffic instance.
        /// </summary>
        private ARVehicle Vehicle => _instances
            .Where(x => x.Type == EEntityType.CopVehicle)
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
            try
            {
                CreateBlip();

                if (BackupType != EBackupUnit.None)
                    Vehicle.GameInstance.IndicatorLightsStatus = VehicleIndicatorLightsStatus.Both;

                Cop.Attach(PropUtils.CreateWand(), PedBoneId.RightPhHand);
                Cop.UnequipAllWeapons();
                AnimationHelper.PlayAnimation(Cop.GameInstance, RedirectTrafficAnimation, "base", AnimationFlags.Loop);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to spawn pursuit roadblock, {ex.Message}", ex);
                return false;
            }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return
                $"{nameof(Position)}: {Position}, {nameof(OffsetPosition)}: {OffsetPosition}, {nameof(Type)}: {Type}, {nameof(BackupType)}: {BackupType}, " +
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
            var rotation = IsLeftSideOfLanes ? -35 : 35;
            LspdfrHelper.CreateBackupUnit(OffsetPosition, Lane.Heading + rotation, BackupType, 1, out var vehicle, out var cops);

            InitializeVehicle(vehicle);
            InitializeCop(cops);
            InitializeScenery();
        }

        private void InitializeVehicle(ARVehicle vehicle)
        {
            if (BackupType == EBackupUnit.None)
            {
                vehicle.Dispose();
            }
            else
            {
                _instances.Add(vehicle);
            }
        }

        private void InitializeCop(IEnumerable<ARPed> cops)
        {
            var distanceBehindVehicle = GetVehicleLength() - 1f;
            var copPedHeading = Lane.Heading - 180;
            var positionBehindVehicle = OffsetPosition + MathHelper.ConvertHeadingToDirection(copPedHeading) * distanceBehindVehicle;
            var cop = cops.First();

            cop.Position = positionBehindVehicle;
            cop.Heading = copPedHeading;

            _instances.Add(cop);
        }

        private void InitializeScenery()
        {
            if (!ConeType.IsNone)
            {
                Logger.Debug($"Placing cone barrier for redirect traffic slot {this}");
                PlaceConesAlongTheRoad();
                PlaceConesBehindTheVehicle();
            }

            PlaceVehiclesStoppedSign();

            if (EnableRedirectionArrow)
                PlaceRedirectionArrow();
            if (EnableLights)
                InitializeVehicleStoppedLight();
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
                _instances.Add(BarrierFactory.Create(ConeType, startPosition, ConeHeading()));
                startPosition += placementDirection * actualConeLength;
            }
        }

        private void PlaceConesBehindTheVehicle()
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
                _instances.Add(BarrierFactory.Create(ConeType, startPosition, ConeHeading()));
                startPosition += placementDirection * coneDistance;
            }
        }

        private void PlaceVehiclesStoppedSign()
        {
            var signPosition = VehicleStoppedSignPosition();

            _instances.Add(new ARScenery(PropUtils.StoppedVehiclesSign(signPosition, Lane.Heading)));
        }

        private void PlaceRedirectionArrow()
        {
            var signPosition = PositionBehindTheVehicle()
                               + MathHelper.ConvertHeadingToDirection(Lane.Heading - 180) * 3f;

            _instances.Add(new ARScenery(IsLeftSideOfLanes
                ? PropUtils.CreateWorkerBarrierArrowRight(signPosition, Lane.Heading)
                : PropUtils.RedirectTrafficArrowLeft(signPosition, Lane.Heading)));
        }

        private Vector3 VehicleStoppedSignPosition()
        {
            var signPosition = PositionBehindTheVehicle()
                               + MathHelper.ConvertHeadingToDirection(Lane.Heading - 180) * 5f;
            return signPosition;
        }

        private void InitializeVehicleStoppedLight()
        {
            var groundLightPosition = VehicleStoppedSignPosition() +
                                      MathHelper.ConvertHeadingToDirection(Lane.Heading - 180) * 1.5f;

            _instances.Add(new ARScenery(PropUtils.CreateGroundFloodLight(groundLightPosition, Lane.Heading - 180)));
        }

        private void CreateBlip()
        {
            if (_blip != null)
                return;

            Logger.Trace($"Creating redirect traffic blip at {OffsetPosition}");
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

            if (BackupType != EBackupUnit.None)
            {
                vehicleWidth = Vehicle.Model.Dimensions.X;
                vehicleLength = Vehicle.Model.Dimensions.Y;
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
            if (ConeType.Barrier.ScriptName.Equals(ConeWithLightName))
                return Lane.Heading - 90;

            return Lane.Heading;
        }

        private float GetVehicleWidth()
        {
            return BackupType == EBackupUnit.None ? DefaultVehicleWidth : Vehicle.Model.Dimensions.X;
        }

        private float GetVehicleLength()
        {
            return BackupType == EBackupUnit.None ? DefaultVehicleLength : Vehicle.Model.Dimensions.Y;
        }

        private Vector3 PositionBasedOnType()
        {
            var lanePosition = Lane.Position;
            var shoulderRotation = IsLeftSideOfLanes ? 90 : -90;

            if (Type == RedirectTrafficType.Shoulder)
                lanePosition += MathHelper.ConvertHeadingToDirection(Lane.Heading + shoulderRotation) * (Lane.Width / 2);

            return lanePosition;
        }

        private Vector3 PositionBehindTheVehicle()
        {
            var position = IsLeftSideOfLanes ? Lane.LeftSide : Lane.RightSide;
            return position
                   + MathHelper.ConvertHeadingToDirection(Lane.Heading - 180) * GetVehicleLength();
        }

        private bool IsLeftSideOfLanesInTheSameHeadingAsTheSelectedLane()
        {
            var distanceToLeftSide = Lane.Position.DistanceTo(Road.LeftSide);
            var distanceToRightSide = Lane.Position.DistanceTo(Road.RightSide);

            Logger.Debug($"Left side closer: {distanceToLeftSide < distanceToRightSide}, " +
                         $"Right side closer: {distanceToRightSide < distanceToLeftSide}, " +
                         $"Is lane opposite: {Lane.IsOppositeHeadingOfRoadNodeHeading}");
            var isLeftSideCloser = distanceToLeftSide < distanceToRightSide;

            if (Lane.IsOppositeHeadingOfRoadNodeHeading)
                isLeftSideCloser = !isLeftSideCloser;

            return isLeftSideCloser;
        }

        #endregion

        public class Request
        {
            public Road Road { get; set; }

            public EBackupUnit BackupType { get; set; }

            public BarrierModel ConeType { get; set; }

            public RedirectTrafficType Type { get; set; }

            public float ConeDistance { get; set; }

            public bool EnableRedirectionArrow { get; set; }

            public bool EnableLights { get; set; }

            public float Offset { get; set; }
        }
    }
}