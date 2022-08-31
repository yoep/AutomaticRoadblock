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

        public RedirectTraffic(Road road, VehicleType vehicleType, BarrierType coneType, RedirectTrafficType type, float coneDistance)
        {
            Assert.NotNull(road, "road cannot be null");
            Assert.NotNull(vehicleType, "vehicleType cannot be null");
            Assert.NotNull(coneType, "coneType cannot be null");
            Assert.NotNull(type, "type cannot be null");
            Road = road;
            Lane = GetLaneClosestToPlayer();
            VehicleType = vehicleType;
            ConeType = coneType;
            Type = type;
            ConeDistance = coneDistance;

            Init();
        }

        #region Properties

        /// <summary>
        /// The position of the redirect traffic instance.
        /// </summary>
        public Vector3 Position => PositionBasedOnType();

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
        /// The vehicle model to use for the vehicle within this instance.
        /// </summary>
        private Model VehicleModel { get; set; }

        /// <summary>
        /// Check if the current traffic redirection is on the left side of the road.
        /// </summary>
        private bool IsLeftSide => Lane.Position.DistanceTo(Road.LeftSide) < Lane.Position.DistanceTo(Road.RightSide);

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
        public void Spawn()
        {
            CreateBlip();
            _instances.ForEach(x => x.Spawn());

            Vehicle.GameInstance.IndicatorLightsStatus = VehicleIndicatorLightsStatus.Both;
            Cop.Attach(PropUtils.CreateWand(), PedBoneId.RightPhHand);
            AnimationUtils.PlayAnimation(Cop.GameInstance, RedirectTrafficAnimation, "base", AnimationFlags.Loop);
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            // Cop.ClearAllTasks();
            // Cop.DeleteAttachments();
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

            VehicleModel = VehicleFactory.CreateModel(VehicleType, Position);

            _instances.Add(new InstanceSlot(EntityType.CopVehicle, Position, Lane.Heading + 35,
                (position, heading) => VehicleFactory.CreateWithModel(VehicleModel, position, heading)));
        }

        private void InitializeCop()
        {
            var distanceBehindVehicle = GetVehicleLength() - 1f;
            var copPedHeading = Lane.Heading - 180;
            var positionBehindVehicle = Position + MathHelper.ConvertHeadingToDirection(copPedHeading) * distanceBehindVehicle;

            _instances.Add(new InstanceSlot(EntityType.CopPed, positionBehindVehicle, copPedHeading,
                (position, heading) => CreateCop(position, heading)));
        }

        private void InitializeScenery()
        {
            PlaceConesAlongTheRoad();
            PlaceConesBehindTheVehicle();
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
            var startPosition = Position + ConeStartDirection();
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

        private void PlaceConesBehindTheVehicle()
        {
            var coneDistance = ConeType.Width + ConeType.Spacing;
            var totalCones = (int)Math.Floor(Lane.Width / coneDistance);
            var placementDirection = MathHelper.ConvertHeadingToDirection(Lane.Heading - 180) * coneDistance +
                                     MathHelper.ConvertHeadingToDirection(Lane.Heading - 90) * coneDistance;
            var startPosition = Position + ConeStartDirection(0.5f);

            Logger.Trace($"Creating a total of {totalCones} cones behind the vehicle for a lane width of {Lane.Width}");
            for (var i = 0; i < totalCones; i++)
            {
                _instances.Add(new InstanceSlot(EntityType.Scenery, startPosition, ConeHeading(),
                    (position, heading) => BarrierFactory.Create(ConeType, position, heading)));
                startPosition += placementDirection * coneDistance;
            }
        }

        private void CreateBlip()
        {
            if (_blip != null)
                return;

            Logger.Trace("Creating roadblock blip");
            _blip = new Blip(Position)
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

        private Vector3 ConeStartDirection(float additionalDistanceBehindVehicle = 0f)
        {
            var vehicleWidth = GetVehicleWidth() + 0.5f;
            var vehicleLength = GetVehicleLength() - 1f;
            var placementSide = IsLeftSide ? -90 : 90;

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

            if (Type == RedirectTrafficType.Shoulder)
                lanePosition += MathHelper.ConvertHeadingToDirection(Lane.Heading - 90) * (Lane.Width / 2);

            return lanePosition;
        }

        #endregion
    }
}