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
        private const float MinimumVehicleSpeed = 20f;
        private const float MinimumRoadblockPlacementDistance = 125f;

        private readonly ILogger _logger;
        private readonly IGame _game;
        private readonly ISettingsManager _settingsManager;

        private readonly List<IRoadblock> _roadblocks = new();

        public RoadblockDispatcher(ILogger logger, IGame game, ISettingsManager settingsManager)
        {
            _logger = logger;
            _game = game;
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

                        _game.DisplayNotification($"Dispatching ~b~roadblock~s~ at {World.GetStreetName(road.Position)}");
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

                _game.DisplayNotification($"Dispatching ~b~roadblock~s~ at {World.GetStreetName(road.Position)}");
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
            _logger.Trace($"Disposing {_roadblocks.Count} roadblock(s)");
            _roadblocks.ForEach(x => x.Dispose());
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
            var distance = vehicleSpeed * 2.5f;

            if (distance < MinimumRoadblockPlacementDistance)
                distance = MinimumRoadblockPlacementDistance;

            return distance;
        }

        private void RoadblockStateChanged(IRoadblock roadblock, RoadblockState newState)
        {
            _game.NewSafeFiber(() =>
            {
                _logger.Debug($"Roadblock state changed to {newState}");
                switch (newState)
                {
                    case RoadblockState.Hit:
                        _game.DisplayNotification("~g~Roadblock has been hit");
                        LspdfrUtils.PlayScannerAudio("REPORT_SUSPECT_CRASHED_VEHICLE");
                        break;
                    case RoadblockState.Bypassed:
                        _game.DisplayNotification("~r~Roadblock has been bypassed");
                        break;
                    case RoadblockState.Disposed:
                        _logger.Trace($"Removing roadblock {roadblock} from dispatcher");
                        _roadblocks.Remove(roadblock);
                        break;
                }
            }, "RoadblockDispatcher.RoadblockStateChanged");
        }
    }
}