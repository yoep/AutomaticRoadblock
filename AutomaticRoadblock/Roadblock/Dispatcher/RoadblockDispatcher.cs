using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Pursuit.Factory;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.SpikeStrip.Dispatcher;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Dispatcher
{
    public class RoadblockDispatcher : IRoadblockDispatcher
    {
        private const float MinimumVehicleSpeed = 20f;
        private const float MinimumRoadblockPlacementDistance = 175f;
        private const int AutoCleanRoadblockAfterSeconds = 45;
        private const float RoadblockCleanupDistanceFromPlayer = 100f;
        private const float MinimumDistanceBetweenRoadblocks = 10f;
        private const string AudioRequestDenied = "ROADBLOCK_REQUEST_DENIED";
        private const string AudioRequestConfirmed = "ROADBLOCK_REQUEST_CONFIRMED";
        private const string AudioRoadblockBypassed = "ROADBLOCK_BYPASSED";
        private const string AudioRoadblockHit = "ROADBLOCK_HIT";

        private static readonly Random Random = new();

        private readonly ILogger _logger;
        private readonly IGame _game;
        private readonly ISettingsManager _settingsManager;
        private readonly ILocalizer _localizer;
        private readonly ISpikeStripDispatcher _spikeStripDispatcher;

        private readonly List<RoadblockInfo> _roadblocks = new();
        private readonly List<Road> _foundRoads = new();

        private bool _cleanerRunning;
        private bool _userRequestedRoadblockDispatching;

        public RoadblockDispatcher(ILogger logger, IGame game, ISettingsManager settingsManager, ILocalizer localizer,
            ISpikeStripDispatcher spikeStripDispatcher)
        {
            _logger = logger;
            _game = game;
            _settingsManager = settingsManager;
            _localizer = localizer;
            _spikeStripDispatcher = spikeStripDispatcher;
        }

        #region Events

        /// <inheritdoc />
        public event RoadblockEvents.RoadblockStateChanged RoadblockStateChanged;

        /// <inheritdoc />
        public event RoadblockEvents.RoadblockCopKilled RoadblockCopKilled;

        /// <inheritdoc />
        public event RoadblockEvents.RoadblockCopsJoiningPursuit RoadblockCopsJoiningPursuit;

        #endregion

        #region IRoadblockDispatcher

        /// <inheritdoc />
        public IRoadblock Dispatch(RoadblockLevel level, Vehicle vehicle, DispatchOptions options)
        {
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");

            _logger.Trace(
                $"Starting roadblock dispatching with {nameof(level)}: {level}, {nameof(options)}: {options}");
            if (options.Force || options.IsUserRequested || IsRoadblockDispatchingAllowed(vehicle))
                return DoInternalDispatch(level, vehicle, options);

            _logger.Info($"Dispatching of a roadblock is not allowed with {nameof(level)}: {level}, {nameof(options)}: {options}");
            return null;
        }

        /// <inheritdoc />
        public IRoadblock DispatchPreview(RoadblockLevel level, Vehicle vehicle, DispatchOptions options)
        {
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");

            _logger.Debug($"Dispatching new roadblock preview with options: {options}");
            var roads = DetermineRoadblockLocationPreview(level, vehicle, options.AtCurrentLocation);
            var road = roads.Last();
            _logger.Trace($"Dispatching roadblock on {road}");

            _game.DisplayNotification(_localizer[LocalizationKey.RoadblockDispatchedAt, World.GetStreetName(road.Position)]);
            var actualLevelToUse = DetermineRoadblockLevelBasedOnTheRoadLocation(level, road);
            var roadblock = PursuitRoadblockFactory.Create(_spikeStripDispatcher, actualLevelToUse, road, vehicle, _settingsManager.AutomaticRoadblocksSettings.SlowTraffic,
                ShouldAddLightsToRoadblock(), ShouldPlaceSpikeStripInRoadblock(options.EnableSpikeStrips));

            lock (_roadblocks)
            {
                _roadblocks.Add(new RoadblockInfo(roadblock));
            }

            _game.NewSafeFiber(() =>
            {
                roadblock.CreatePreview();
                lock (_foundRoads)
                {
                    _foundRoads.AddRange(roads);
                    _foundRoads.ForEach(x => x.CreatePreview());
                }
            }, "RoadblockDispatcher.DispatchPreview");

            return roadblock;
        }

        /// <inheritdoc />
        public void DismissActiveRoadblocks()
        {
            _logger.Debug("Dismissing any active roadblocks");
            List<IRoadblock> roadblocksToRelease;

            lock (_roadblocks)
            {
                roadblocksToRelease = _roadblocks
                    .Select(x => x.Roadblock)
                    .Where(x => x.State == ERoadblockState.Active)
                    .ToList();
            }

            // only release the roadblock and don't remove it yet
            // this will change the state of the roadblock to released state allowing
            // it to be picked up by the cleanup thread which will dispose the roadblock correctly
            roadblocksToRelease.ForEach(x => x.Release());
            _logger.Info($"Released a total of {roadblocksToRelease.Count} roadblocks which were still active");
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _logger.Trace($"Disposing {_roadblocks.Count} remaining roadblocks during shutdown");
            _cleanerRunning = false;
            _roadblocks
                .Select(x => x.Roadblock)
                .ToList()
                .ForEach(x => x.Dispose());
            _roadblocks.Clear();
            _foundRoads.ForEach(x => x.DeletePreview());
            _foundRoads.Clear();
            _logger.Info("Remaining roadblocks have been disposed during shutdown");
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
                   GameUtils.TimePeriod is ETimePeriod.Evening or ETimePeriod.Night;
        }

        private bool ShouldPlaceSpikeStripInRoadblock(bool enableSpikeStrips)
        {
            var spawnChance = _settingsManager.AutomaticRoadblocksSettings.SpikeStripChance * 100;
            var threshold = Random.Next(101);
            
            return enableSpikeStrips && spawnChance >= threshold;
        }

        private IRoadblock DoInternalDispatch(RoadblockLevel level, Vehicle vehicle, DispatchOptions options)
        {
            // start the cleaner if it's not yet running
            if (!_cleanerRunning)
                StartCleaner();

            // verify if a user requested roadblock is still being dispatched
            // because of the fact that a user requested roadblock plays blocking audio,
            // the roadblock might still not have been deployed when a new one is requested
            if (_userRequestedRoadblockDispatching)
            {
                DenyUserRequestForRoadblock(options.IsUserRequested, "user requested roadblock is currently being dispatched");
                return null;
            }

            if (options.IsUserRequested)
                AllowUserRequestForRoadblock();

            _logger.Debug($"Dispatching new roadblock with {nameof(options)}: {options}");
            // calculate the roadblock location
            var road = DetermineRoadblockLocation(level, vehicle, options.AtCurrentLocation);

            // verify if another roadblock is already present nearby
            // if so, deny the roadblock request
            if (IsRoadblockNearby(road))
            {
                DenyUserRequestForRoadblock(options.IsUserRequested, $"a roadblock is already present in the vicinity for {road}");
                return null;
            }

            var actualLevelToUse = DetermineRoadblockLevelBasedOnTheRoadLocation(level, road);
            var roadblock = PursuitRoadblockFactory.Create(_spikeStripDispatcher, actualLevelToUse, road, vehicle, _settingsManager.AutomaticRoadblocksSettings.SlowTraffic,
                ShouldAddLightsToRoadblock(), ShouldPlaceSpikeStripInRoadblock(options.EnableSpikeStrips));

            lock (_roadblocks)
            {
                _roadblocks.Add(new RoadblockInfo(roadblock));
            }

            _game.NewSafeFiber(() =>
                {
                    _logger.Info($"Dispatching new roadblock\n{roadblock}");
                    // subscribe to the roadblock events
                    roadblock.RoadblockStateChanged += InternalRoadblockStateChanged;
                    roadblock.RoadblockCopKilled += InternalRoadblockCopKilled;
                    roadblock.RoadblockCopsJoiningPursuit += InternalRoadblockCopsJoiningThePursuit;

                    _logger.Trace($"Distance between vehicle and roadblock before spawn {road.Position.DistanceTo(vehicle.Position)}");
                    var result = roadblock.Spawn();
                    if (!result)
                        _logger.Warn($"Not all roadblock instances spawned with success for {roadblock}");

                    _logger.Trace($"Distance between vehicle and roadblock after spawn {road.Position.DistanceTo(vehicle.Position)}");
                    _game.DisplayNotification(_localizer[LocalizationKey.RoadblockDispatchedAt, World.GetStreetName(road.Position)]);
                    _logger.Info($"Roadblock has been dispatched, {roadblock}");
                    LspdfrUtils.PlayScannerAudioNonBlocking("ROADBLOCK_DEPLOYED");
                    _userRequestedRoadblockDispatching = false;
                },
                "RoadblockDispatcher.Dispatch");

            return roadblock;
        }

        private bool IsRoadblockNearby(Road road)
        {
            bool isThereANearbyRoadblock;

            lock (_roadblocks)
            {
                isThereANearbyRoadblock = _roadblocks
                    // filter out any previews and roadblocks in error state
                    // as we don't want them to prevent a roadblock placement
                    .Where(x => !x.Roadblock.IsPreviewActive && x.State != ERoadblockState.Error)
                    .Any(x => x.Position.DistanceTo(road.Position) <= MinimumDistanceBetweenRoadblocks);
            }

            return isThereANearbyRoadblock;
        }

        private Road DetermineRoadblockLocation(RoadblockLevel level, Vehicle vehicle, bool atCurrentLocation)
        {
            var roadblockDistance = CalculateRoadblockDistance(vehicle, atCurrentLocation);
            var roadType = DetermineAllowedRoadTypes(vehicle, level);

            _logger.Trace(
                $"Determining roadblock location with Position: {vehicle.Position}, Heading: {vehicle.Heading}, {nameof(roadblockDistance)}: {roadblockDistance}, {nameof(roadType)}: {roadType}");
            return RoadUtils.FindRoadTraversing(vehicle.Position, vehicle.Heading, roadblockDistance, roadType);
        }

        private ICollection<Road> DetermineRoadblockLocationPreview(RoadblockLevel level, Vehicle vehicle, bool atCurrentLocation)
        {
            var roadblockDistance = CalculateRoadblockDistance(vehicle, atCurrentLocation);
            var roadType = DetermineAllowedRoadTypes(vehicle, level);

            _logger.Trace(
                $"Determining roadblock location for the preview with Position: {vehicle.Position}, Heading: {vehicle.Heading}, {nameof(roadblockDistance)}: {roadblockDistance}, {nameof(roadType)}: {roadType}");
            return RoadUtils.FindRoadsTraversing(vehicle.Position, vehicle.Heading, roadblockDistance, roadType);
        }

        private RoadblockLevel DetermineRoadblockLevelBasedOnTheRoadLocation(RoadblockLevel level, Road road)
        {
            var actualLevelToUse = level;
            var isDirtOrOffroad = RoadUtils.IsDirtOrOffroad(road.Position);

            // if we're not a dirt/offroad road
            // all levels are allowed
            _logger.Trace($"Roadblock placement is on dirt/offroad road: {isDirtOrOffroad}");
            if (!isDirtOrOffroad)
                return actualLevelToUse;

            // otherwise, we're going to reduce the level for simplification
            _logger.Debug("Detected a dirt/offroad position for the roadblock");
            if (level.Level > 3)
            {
                actualLevelToUse = RoadblockLevel.Level2;
                _logger.Info($"Roadblock level has been reduced to {RoadblockLevel.Level2} as the location is a dirt/offroad location");
            }

            return actualLevelToUse;
        }

        private void InternalRoadblockStateChanged(IRoadblock roadblock, ERoadblockState newState)
        {
            _logger.Debug($"Roadblock state changed to {newState}");
            _game.NewSafeFiber(() =>
            {
                switch (newState)
                {
                    case ERoadblockState.Hit:
                        _game.DisplayNotification(_localizer[LocalizationKey.RoadblockHasBeenHit]);
                        LspdfrUtils.PlayScannerAudioNonBlocking(AudioRoadblockHit);
                        break;
                    case ERoadblockState.Bypassed:
                        _game.DisplayNotification(_localizer[LocalizationKey.RoadblockHasBeenBypassed]);
                        LspdfrUtils.PlayScannerAudioNonBlocking(AudioRoadblockBypassed);
                        break;
                    case ERoadblockState.Disposed:
                        _logger.Trace($"Removing roadblock {roadblock} from dispatcher");
                        RemoveRoadblock(roadblock);
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

        private void InternalRoadblockCopsJoiningThePursuit(IRoadblock roadblock, IEnumerable<Ped> cops)
        {
            RoadblockCopsJoiningPursuit?.Invoke(roadblock, cops);
        }

        private void StartCleaner()
        {
            _logger.Trace("Starting the roadblock dispatcher cleaner");
            _cleanerRunning = true;
            _game.NewSafeFiber(() =>
            {
                _logger.Info("Roadblock dispatch cleaner started");
                while (_cleanerRunning)
                {
                    // verify if we need to do a cleanup
                    // if there are no roadblocks, skip the cleanup
                    lock (_roadblocks)
                    {
                        if (_roadblocks.Count > 0)
                            DoCleanupTick();
                    }

                    GameFiber.Wait(10 * 1000);
                }

                _logger.Debug("Roadblock dispatch cleaner stopped");
            }, "RoadblockDispatcher.Cleaner");
        }

        private void DoCleanupTick()
        {
            lock (_roadblocks)
            {
                _logger.Trace($"Roadblock cleanup will check a total of {_roadblocks.Count} roadblocks");
                _roadblocks
                    .Where(x => !x.Roadblock.IsPreviewActive && x.State is not ERoadblockState.Active or ERoadblockState.Preparing or ERoadblockState.Disposing)
                    // verify if the player if far enough away for the roadblock to be cleaned
                    // if not, we auto clean roadblocks after AutoCleanRoadblockAfterSeconds
                    .Where(x => IsPlayerFarAwayFromRoadblock(x) || IsAutoRoadblockCleaningAllowed(x.Roadblock))
                    .ToList()
                    .ForEach(x =>
                    {
                        x.Roadblock.Dispose();
                        _logger.Debug($"Roadblock cleanup has disposed roadblock {x}");
                    });
            }
        }

        private bool IsPlayerFarAwayFromRoadblock(RoadblockInfo roadblockInfo)
        {
            var playerPosition = _game.PlayerPosition;
            var currentDistanceToRoadblock = playerPosition.DistanceTo(roadblockInfo.Position);

            // if the player is moving towards the roadblock
            // we don't clean it even if it exceeds the RoadblockCleanupDistanceFromPlayer
            if (roadblockInfo.IsPlayerMovingTowardsRoadblock(currentDistanceToRoadblock))
            {
                _logger.Debug($"Player is moving towards roadblock (distance {currentDistanceToRoadblock}), cleanup not allowed for {roadblockInfo.Roadblock}");
                return false;
            }

            return currentDistanceToRoadblock > RoadblockCleanupDistanceFromPlayer;
        }

        private bool IsAutoRoadblockCleaningAllowed(IRoadblock roadblock)
        {
            return _game.GameTime - roadblock.LastStateChange >= AutoCleanRoadblockAfterSeconds * 1000;
        }

        private void AllowUserRequestForRoadblock()
        {
            _logger.Trace("Playing roadblock requested by user audio");
            _userRequestedRoadblockDispatching = true;
            LspdfrUtils.PlayScannerAudio(AudioRequestConfirmed, true);
        }

        private void DenyUserRequestForRoadblock(bool userRequest, string reason)
        {
            _logger.Warn("Dispatching new roadblock is not allowed, " + reason);

            if (userRequest)
                LspdfrUtils.PlayScannerAudioNonBlocking(AudioRequestDenied);
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

        private EVehicleNodeType DetermineAllowedRoadTypes(Vehicle vehicle, RoadblockLevel level)
        {
            // verify the current road type
            // if we're already at a dirt/offroad road, all road types for the trajectory calculation are allowed
            if (RoadUtils.IsDirtOrOffroad(vehicle.Position))
            {
                _logger.Debug("Following the current dirt/offroad road for the roadblock placement");
                return EVehicleNodeType.AllRoadNoJunctions;
            }

            // otherwise, we're going to base the allowed road types for the trajectory based
            // on the current roadblock level
            var vehicleNodeType = level.Level <= RoadblockLevel.Level2.Level ? EVehicleNodeType.AllRoadNoJunctions : EVehicleNodeType.MainRoads;
            _logger.Debug($"Roadblock road traversal will use vehicle node type {vehicleNodeType}");
            return vehicleNodeType;
        }

        private void RemoveRoadblock(IRoadblock roadblock)
        {
            lock (_roadblocks)
            {
                var roadblockInfo = _roadblocks.FirstOrDefault(x => x.Roadblock == roadblock);

                if (roadblockInfo != null)
                {
                    _roadblocks.Remove(roadblockInfo);
                }
                else
                {
                    _logger.Warn($"Unable to remove roadblock from dispatcher, roadblock not found: {roadblock}");
                }
            }
        }

        #endregion
    }
}