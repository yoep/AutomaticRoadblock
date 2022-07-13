using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Dispatcher
{
    public class RoadblockDispatcher : IRoadblockDispatcher
    {
        private const float MinimumVehicleSpeed = 20f;
        private const float MinimumRoadblockPlacementDistance = 125f;
        private const int AutoCleanRoadblockAfterSeconds = 60;

        private readonly ILogger _logger;
        private readonly IGame _game;
        private readonly ISettingsManager _settingsManager;

        private readonly List<IRoadblock> _roadblocks = new();

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
        public bool Dispatch(RoadblockLevel level, Vehicle vehicle, bool force)
        {
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");

            _logger.Trace($"Starting roadblock dispatching with force state {force}");
            if (force || IsRoadblockDispatchingAllowed(vehicle))
            {
                DoDispose(level, vehicle);
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
                var road = DetermineRoadblockLocation(vehicle, atCurrentLocation);
                _logger.Trace($"Dispatching roadblock on {road}");

                _game.DisplayNotification($"Dispatching ~b~roadblock~s~ at {World.GetStreetName(road.Position)}");
                var roadblock = RoadblockFactory.Create(level, road, vehicle, _settingsManager.AutomaticRoadblocksSettings.SlowTraffic,
                    ShouldAddLightsToRoadblock());
                _logger.Info($"Dispatching new roadblock\n{roadblock}");
                _roadblocks.Add(roadblock);

                roadblock.CreatePreview();
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

        private void DoDispose(RoadblockLevel level, Vehicle vehicle)
        {
            _logger.Debug("Dispatching new roadblock");

            // start the cleaner if it's not yet running
            if (!_cleanerRunning)
                StartCleaner();

            _game.NewSafeFiber(() =>
                {
                    var road = DetermineRoadblockLocation(vehicle);
                    _logger.Trace($"Dispatching roadblock on {road}");

                    _game.DisplayNotification($"Dispatching ~b~roadblock~s~ at {World.GetStreetName(road.Position)}");
                    LspdfrUtils.PlayScannerAudioUsingPosition("WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION", road.Position);

                    var roadblock = RoadblockFactory.Create(level, road, vehicle, _settingsManager.AutomaticRoadblocksSettings.SlowTraffic,
                        ShouldAddLightsToRoadblock());
                    _logger.Info($"Dispatching new roadblock\n{roadblock}");
                    _roadblocks.Add(roadblock);

                    // subscribe to the roadblock events
                    roadblock.RoadblockStateChanged += InternalRoadblockStateChanged;
                    roadblock.RoadblockCopKilled += InternalRoadblockCopKilled;

                    roadblock.Spawn();
                },
                "RoadblockDispatcher.Dispatch");
        }

        private Road DetermineRoadblockLocation(Vehicle vehicle, bool atCurrentLocation = false)
        {
            _logger.Trace("Determining roadblock location");
            var direction = MathHelper.ConvertHeadingToDirection(vehicle.Heading);
            var roadblockDistance = atCurrentLocation ? 2.5f : DetermineRoadblockDistance(vehicle);

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

        #endregion
    }
}