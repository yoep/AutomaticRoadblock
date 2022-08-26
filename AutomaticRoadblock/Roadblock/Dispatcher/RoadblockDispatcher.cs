using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Pursuit.Factory;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Dispatcher
{
    public class RoadblockDispatcher : IRoadblockDispatcher
    {
        private const float MinimumVehicleSpeed = 20f;
        private const float MinimumRoadblockPlacementDistance = 175f;
        private const int AutoCleanRoadblockAfterSeconds = 60;

        private readonly ILogger _logger;
        private readonly IGame _game;
        private readonly ISettingsManager _settingsManager;

        private readonly List<IRoadblock> _roadblocks = new();
        private readonly List<Road> _foundRoads = new();

        private bool _cleanerRunning;

        public RoadblockDispatcher(ILogger logger, IGame game, ISettingsManager settingsManager)
        {
            _logger = logger;
            _game = game;
            _settingsManager = settingsManager;
        }

        #region Properties

        /// <inheritdoc />
        public IEnumerable<IRoadblock> Roadblocks => _roadblocks;

        #endregion

        #region Events

        /// <inheritdoc />
        public event RoadblockEvents.RoadblockStateChanged RoadblockStateChanged;

        /// <inheritdoc />
        public event RoadblockEvents.RoadblockCopKilled RoadblockCopKilled;

        #endregion

        #region IRoadblockDispatcher

        /// <inheritdoc />
        public bool Dispatch(RoadblockLevel level, Vehicle vehicle, bool userRequested, bool force, bool atCurrentLocation = false)
        {
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");

            _logger.Trace(
                $"Starting roadblock dispatching with {nameof(level)}: {level}, {nameof(userRequested)}: {userRequested}, {nameof(force)}: {force}, {nameof(atCurrentLocation)}: {atCurrentLocation}");
            if (force || userRequested || IsRoadblockDispatchingAllowed(vehicle))
            {
                DoDispose(level, vehicle, atCurrentLocation);
                return true;
            }

            _logger.Debug("Dispatching of a roadblock is not allowed");
            return false;
        }

        /// <inheritdoc />
        public void DispatchPreview(RoadblockLevel level, Vehicle vehicle, bool atCurrentLocation)
        {
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");

            _game.NewSafeFiber(() =>
            {
                _logger.Debug("Dispatching new roadblock preview");
                var roads = DetermineRoadblockLocationPreview(level, vehicle, atCurrentLocation);
                var road = roads.Last();
                _logger.Trace($"Dispatching roadblock on {road}");

                _game.DisplayNotification($"Dispatching ~b~roadblock~s~ at {World.GetStreetName(road.Position)}");
                var roadblock = PursuitRoadblockFactory.Create(level, road, vehicle, _settingsManager.AutomaticRoadblocksSettings.SlowTraffic,
                    ShouldAddLightsToRoadblock());

                _roadblocks.Add(roadblock);
                _foundRoads.AddRange(roads);

                roadblock.CreatePreview();
                _foundRoads.ForEach(x => x.CreatePreview());
            }, "RoadblockDispatcher.DispatchPreview");
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _logger.Trace($"Disposing {_roadblocks.Count} roadblock(s)");
            _cleanerRunning = false;
            _roadblocks.ForEach(x => x.Dispose());
            _roadblocks.Clear();
            _foundRoads.ForEach(x => x.DeletePreview());
            _foundRoads.Clear();
            _logger.Debug("Roadblocks have been disposed");
        }

        #endregion

        #region Functions

        private bool IsRoadblockDispatchingAllowed(Vehicle vehicle)
        {
            return vehicle.Speed >= MinimumVehicleSpeed;
        }

        private bool ShouldAddLightsToRoadblock()
        {
            return _settingsManager.AutomaticRoadblocksSettings.EnableLights &&
                   GameUtils.TimePeriod is TimePeriod.Evening or TimePeriod.Night;
        }

        private void DoDispose(RoadblockLevel level, Vehicle vehicle, bool atCurrentLocation)
        {
            _logger.Debug($"Dispatching new roadblock with {nameof(atCurrentLocation)}: {atCurrentLocation}");

            // start the cleaner if it's not yet running
            if (!_cleanerRunning)
                StartCleaner();

            _game.NewSafeFiber(() =>
                {
                    var road = DetermineRoadblockLocation(level, vehicle, atCurrentLocation);
                    _logger.Trace($"Dispatching roadblock on {road}");
                    
                    var roadblock = PursuitRoadblockFactory.Create(level, road, vehicle, _settingsManager.AutomaticRoadblocksSettings.SlowTraffic,
                        ShouldAddLightsToRoadblock());
                    _logger.Info($"Dispatching new roadblock\n{roadblock}");
                    _roadblocks.Add(roadblock);

                    // subscribe to the roadblock events
                    roadblock.RoadblockStateChanged += InternalRoadblockStateChanged;
                    roadblock.RoadblockCopKilled += InternalRoadblockCopKilled;

                    _logger.Trace($"Distance between vehicle and roadblock before spawn {road.Position.DistanceTo(vehicle.Position)}");
                    roadblock.Spawn();
                    _logger.Trace($"Distance between vehicle and roadblock after spawn {road.Position.DistanceTo(vehicle.Position)}");
                    _game.DisplayNotification($"Dispatching ~b~roadblock~s~ at {World.GetStreetName(road.Position)}");
                    LspdfrUtils.PlayScannerAudio("ROADBLOCK_DEPLOYED");
                },
                "RoadblockDispatcher.Dispatch");
        }

        private Road DetermineRoadblockLocation(RoadblockLevel level, Vehicle vehicle, bool atCurrentLocation)
        {
            var roadblockDistance = CalculateRoadblockDistance(vehicle, atCurrentLocation);
            var roadType = DetermineAllowedRoadTypes(level);

            _logger.Trace($"Determining roadblock location with {nameof(roadblockDistance)}: {roadblockDistance}, {nameof(roadType)}: {roadType}");
            return RoadUtils.FindRoadTraversing(vehicle.Position, vehicle.Heading, roadblockDistance, roadType);
        }

        private ICollection<Road> DetermineRoadblockLocationPreview(RoadblockLevel level, Vehicle vehicle, bool atCurrentLocation)
        {
            var roadblockDistance = CalculateRoadblockDistance(vehicle, atCurrentLocation);
            var roadType = DetermineAllowedRoadTypes(level);

            _logger.Trace(
                $"Determining roadblock location for the preview with {nameof(roadblockDistance)}: {roadblockDistance}, {nameof(roadType)}: {roadType}");
            return RoadUtils.FindRoadsTraversing(vehicle.Position, vehicle.Heading, roadblockDistance, roadType);
        }

        private void InternalRoadblockStateChanged(IRoadblock roadblock, RoadblockState newState)
        {
            _logger.Debug($"Roadblock state changed to {newState}");
            _game.NewSafeFiber(() =>
            {
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
            RoadblockStateChanged?.Invoke(roadblock, newState);
        }

        private void InternalRoadblockCopKilled(IRoadblock roadblock)
        {
            _logger.Debug($"A roadblock cop has been killed");
            RoadblockCopKilled?.Invoke(roadblock);
        }

        private void StartCleaner()
        {
            _logger.Trace("Starting the roadblock dispatcher cleaner");
            _cleanerRunning = true;
            _game.NewSafeFiber(() =>
            {
                while (_cleanerRunning)
                {
                    _roadblocks
                        .Where(x => x.State is not RoadblockState.Active or RoadblockState.Preparing)
                        .Where(x => _game.GameTime - x.LastStateChange >= AutoCleanRoadblockAfterSeconds * 1000)
                        .ToList()
                        .ForEach(x => x.Dispose());
                    GameFiber.Wait(15 * 1000);
                }
            }, "RoadblockDispatcher.StartCleaner");
        }

        private static float CalculateRoadblockDistance(Vehicle vehicle, bool atCurrentLocation)
        {
            return atCurrentLocation ? 2.5f : DetermineRoadblockDistanceFor(vehicle);
        }

        private static float DetermineRoadblockDistanceFor(Vehicle vehicle)
        {
            var vehicleSpeed = vehicle.Speed;
            var distance = vehicleSpeed * 3.5f;

            if (distance < MinimumRoadblockPlacementDistance)
                distance = MinimumRoadblockPlacementDistance;

            return distance;
        }

        private static VehicleNodeType DetermineAllowedRoadTypes(RoadblockLevel level)
        {
            return level.Level <= RoadblockLevel.Level2.Level ? VehicleNodeType.AllRoadNoJunctions : VehicleNodeType.MainRoads;
        }

        #endregion
    }
}