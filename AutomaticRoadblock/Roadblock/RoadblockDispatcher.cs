using System.Collections.Generic;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    public class RoadblockDispatcher : IRoadblockDispatcher
    {
        private const float MinimumVehicleSpeed = 15f;
        private const float MinimumRoadblockPlacementDistance = 60f;

        private readonly ILogger _logger;
        private readonly IGame _game;
        private readonly INotification _notification;
        private readonly ISettingsManager _settingsManager;

        private readonly List<IRoadblock> _roadblocks = new List<IRoadblock>();

        public RoadblockDispatcher(ILogger logger, IGame game, INotification notification, ISettingsManager settingsManager)
        {
            _logger = logger;
            _game = game;
            _notification = notification;
            _settingsManager = settingsManager;
        }

        /// <inheritdoc />
        public bool Dispatch(RoadblockLevel level, Vehicle vehicle, bool force)
        {
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");

            _logger.Trace($"Starting roadblock dispatching with force state {force}");
            if (force || IsRoadblockDispatchingAllowed(vehicle))
            {
                _logger.Debug("Dispatching new roadblock");
                _game.NewSafeFiber(() =>
                    {
                        var road = DetermineRoadblockLocation(vehicle);
                        _logger.Trace($"Dispatching roadblock on {road}");

                        _notification.DisplayNotification($"Dispatching ~b~roadblock~s~ at {World.GetStreetName(road.Position)}");
                        LspdfrUtils.PlayScannerAudioUsingPosition("WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION", road.Position);

                        var roadblock = new Roadblock(level, road, vehicle, _settingsManager.AutomaticRoadblocksSettings.SlowTraffic);
                        _logger.Info($"Dispatching new roadblock\n{roadblock}");
                        _roadblocks.Add(roadblock);

                        roadblock.RoadblockStateChanged += RoadblockStateChanged;
                        roadblock.Spawn();
                    },
                    "RoadblockDispatcher.Dispatch");
                return true;
            }

            _logger.Debug("Dispatching of a roadblock is not allowed");
            return false;
        }

        /// <inheritdoc />
        public void DispatchPreview(RoadblockLevel level, Vehicle vehicle)
        {
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");

            _game.NewSafeFiber(() =>
            {
                _logger.Debug("Dispatching new roadblock preview");
                var road = DetermineRoadblockLocation(vehicle);
                _logger.Trace($"Dispatching roadblock on {road}");
                
                _notification.DisplayNotification($"Dispatching ~b~roadblock~s~ at {World.GetStreetName(road.Position)}");
                var roadblock = new Roadblock(level, road, vehicle, _settingsManager.AutomaticRoadblocksSettings.SlowTraffic);
                _logger.Info($"Dispatching new roadblock\n{roadblock}");
                _roadblocks.Add(roadblock);

                roadblock.CreatePreview();
            }, "RoadblockDispatcher.DispatchPreview");
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _logger.Trace("Disposing roadblocks");
            foreach (var roadblock in _roadblocks)
            {
                roadblock.Dispose();
            }

            _roadblocks.Clear();
            _logger.Debug("Roadblocks have been disposed");
        }

        #endregion

        private bool IsRoadblockDispatchingAllowed(Vehicle vehicle)
        {
            return vehicle.Speed >= MinimumVehicleSpeed;
        }

        private Road DetermineRoadblockLocation(Vehicle vehicle)
        {
            _logger.Trace("Determining roadblock location");
            var direction = MathHelper.ConvertHeadingToDirection(vehicle.Heading);
            var roadblockDistance = DetermineRoadblockDistance(vehicle);

            return RoadUtils.GetClosestRoad(vehicle.Position + direction * roadblockDistance, RoadType.All);
        }

        private float DetermineRoadblockDistance(Vehicle vehicle)
        {
            var vehicleSpeed = vehicle.Speed;
            var distance = vehicleSpeed * 2f;

            if (distance < MinimumRoadblockPlacementDistance)
                distance = MinimumRoadblockPlacementDistance;

            return distance;
        }

        private void RoadblockStateChanged(RoadblockState newState)
        {
            _logger.Debug($"Roadblock state changed to {newState}");
            switch (newState)
            {
                case RoadblockState.Hit:
                    _notification.DisplayNotification("~g~Roadblock has been hit");
                    break;
                case RoadblockState.Bypassed:
                    _notification.DisplayNotification("~r~Roadblock has been bypassed");
                    break;
            }
        }
    }
}