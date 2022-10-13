using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Pursuit.Factory;
using AutomaticRoadblocks.Pursuit.Level;
using AutomaticRoadblocks.Roadblock.Data;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Street;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using JetBrains.Annotations;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Dispatcher
{
    internal class RoadblockDispatcher : IRoadblockDispatcher
    {
        private const float MinimumVehicleSpeed = 20f;
        private const float MinimumRoadblockPlacementDistance = 175f;
        private const int AutoCleanRoadblockAfterSeconds = 45;
        private const float RoadblockCleanupDistanceFromPlayer = 100f;
        private const float MinimumDistanceBetweenRoadblocks = 10f;
        private const float MinimumDistanceFromTargetForJunctionRoadblocks = 60f;
        private const float MinimumDistanceSpawningAwayFromSuspect = 45f;
        private const string AudioRequestDenied = "ROADBLOCK_REQUEST_DENIED";
        private const string AudioRequestConfirmed = "ROADBLOCK_REQUEST_CONFIRMED";

        private static readonly Random Random = new();

        private readonly ILogger _logger;
        private readonly IGame _game;
        private readonly ISettingsManager _settingsManager;
        private readonly ILocalizer _localizer;
        private readonly IRoadblockData _roadblockData;

        private readonly List<RoadblockInfo> _roadblocks = new();
        private readonly List<IVehicleNode> _foundRoads = new();

        private bool _cleanerRunning;
        private bool _userRequestedRoadblockDispatching;

        public RoadblockDispatcher(ILogger logger, IGame game, ISettingsManager settingsManager, ILocalizer localizer, IRoadblockData roadblockData)
        {
            _logger = logger;
            _game = game;
            _settingsManager = settingsManager;
            _localizer = localizer;
            _roadblockData = roadblockData;
        }

        #region Properties

        /// <summary>
        /// The roadblock settings of the plugin.
        /// </summary>
        private AutomaticRoadblocksSettings Settings => _settingsManager.AutomaticRoadblocksSettings;

        /// <summary>
        /// Verify if junction roadblocks are enabled.
        /// </summary>
        private bool IsJunctionRoadblockEnabled => Settings.EnableIntersectionRoadblocks;

        #endregion

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
        public IRoadblock Dispatch(ERoadblockLevel level, Vehicle vehicle, DispatchOptions options)
        {
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");

            _logger.Trace(
                $"Starting roadblock dispatching with {nameof(level)}: {level}, {nameof(options)}: {options}");
            if (options.Force || options.IsUserRequested || IsRoadblockDispatchingAllowed(vehicle))
                return DoInternalDispatch(level, vehicle, options);

            _logger.Info($"Dispatching of a roadblock is not allowed with {nameof(level)}: {level}, {nameof(options)}: {options}, " +
                         $"{nameof(IsRoadblockDispatchingAllowed)}: {IsRoadblockDispatchingAllowed(vehicle)}");
            return null;
        }

        /// <inheritdoc />
        public IRoadblock DispatchPreview(ERoadblockLevel level, Vehicle vehicle, DispatchOptions options)
        {
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");
            _logger.Debug($"Dispatching new roadblock preview with options: {options}");
            var roads = DetermineRoadblockLocation(level, vehicle, options.RoadblockDistance);
            _logger.Trace($"Dispatching roadblock on {roads.Last()}");

            _game.DisplayNotification(_localizer[LocalizationKey.RoadblockDispatchedAt, World.GetStreetName(roads.Last().Position)]);
            _game.NewSafeFiber(() =>
            {
                lock (_foundRoads)
                {
                    _foundRoads.AddRange(roads);
                    _foundRoads.ForEach(x => x.CreatePreview());
                }
            }, "RoadblockDispatcher.DispatchPreview");
            return DoRoadblockCreations(roads, vehicle, level, true, options);
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
            roadblocksToRelease.ForEach(x => x.Release(true));
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
            return Settings.EnableLights &&
                   GameUtils.TimePeriod is ETimePeriod.Evening or ETimePeriod.Night;
        }

        private bool ShouldPlaceSpikeStripInRoadblock(bool enableSpikeStrips)
        {
            var spawnChance = _settingsManager.AutomaticRoadblocksSettings.SpikeStripChance * 100;
            var threshold = Random.Next(101);

            _logger.Trace($"Spike strip change Enabled: {enableSpikeStrips}, {nameof(spawnChance)}: {spawnChance}, {nameof(threshold)}: {threshold}");
            return enableSpikeStrips && spawnChance >= threshold;
        }

        [CanBeNull]
        private IRoadblock DoInternalDispatch(ERoadblockLevel level, Vehicle vehicle, DispatchOptions options)
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

            // update the flag that a user requested roadblock is being calculated
            if (options.IsUserRequested)
                _userRequestedRoadblockDispatching = true;

            // wait for all audio to complete before doing the initial calculation
            LspdfrUtils.WaitForAudioCompletion();

            // calculate the roadblock location
            _logger.Debug($"Dispatching new roadblock with {nameof(options)}: {options}");
            var discoveredVehicleNodes = DetermineRoadblockLocation(level, vehicle, options.RoadblockDistance);
            var primaryRoadblockNode = discoveredVehicleNodes.OfType<Road>().Last();

            if (primaryRoadblockNode.Width < 1f)
            {
                DenyUserRequestForRoadblock(options.IsUserRequested, $"roadblock location is invalid for {primaryRoadblockNode} (road width too small)");
                _userRequestedRoadblockDispatching = false;
                return null;
            }

            var distanceFromSuspect = primaryRoadblockNode.Position.DistanceTo2D(vehicle.Position);
            _logger.Trace($"Roadblock calculated distance from suspect {distanceFromSuspect}");
            if (distanceFromSuspect < MinimumDistanceSpawningAwayFromSuspect)
            {
                DenyUserRequestForRoadblock(options.IsUserRequested, $"roadblock location is invalid for {primaryRoadblockNode} (min spawn distance not met)");
                _userRequestedRoadblockDispatching = false;
                return null;
            }

            if (options.IsUserRequested)
                AllowUserRequestForRoadblock();

            // verify if another roadblock is already present nearby
            // if so, deny the roadblock request
            if (IsRoadblockNearby(primaryRoadblockNode))
            {
                DenyUserRequestForRoadblock(options.IsUserRequested, $"a roadblock is already present in the vicinity for {discoveredVehicleNodes}");
                _userRequestedRoadblockDispatching = false;
                return null;
            }

            var primaryRoadblock = DoRoadblockCreations(discoveredVehicleNodes, vehicle, level, false, options);
            _userRequestedRoadblockDispatching = false;
            return primaryRoadblock;
        }

        private IRoadblock DoRoadblockCreations(IList<IVehicleNode> streetNodes, Vehicle vehicle, ERoadblockLevel level, bool createAsPreview,
            DispatchOptions options)
        {
            if (IsJunctionRoadblockEnabled)
            {
                var junctionRoads = streetNodes
                    .Where(x => x.Type == EStreetType.Intersection)
                    .Where(x => x.Position.DistanceTo(vehicle.Position) >= MinimumDistanceFromTargetForJunctionRoadblocks)
                    .OfType<Intersection>()
                    .SelectMany(x => FilterRoadsTravellingAlongTheRoute(GetPreviousNode(streetNodes, x), x))
                    .ToList();

                junctionRoads.ForEach(x => CreateRoadblock(x, vehicle, ERoadblockLevel.Level3, createAsPreview,
                    ERoadblockFlags.DetectBypass | ERoadblockFlags.JoinPursuitOnHit | ERoadblockFlags.ForceInVehicle));
                _logger.Info($"Deployed an additional {junctionRoads.Count} junction roadblocks along the road");
            }

            var flags = ERoadblockFlags.JoinPursuit | ERoadblockFlags.PlayAudio;

            if (options.EnableSpikeStrips)
                flags |= ERoadblockFlags.EnableSpikeStrips;

            return CreateRoadblock(streetNodes.OfType<Road>().Last(), vehicle, level, createAsPreview, flags);
        }

        private IRoadblock CreateRoadblock(Road road, Vehicle vehicle, ERoadblockLevel level, bool createAsPreview, ERoadblockFlags flags)
        {
            // add additional flags based on settings & world data
            if (Settings.SlowTraffic)
                flags |= ERoadblockFlags.LimitSpeed;
            if (ShouldAddLightsToRoadblock())
                flags |= ERoadblockFlags.EnableLights;
            if (ShouldPlaceSpikeStripInRoadblock(flags.HasFlag(ERoadblockFlags.EnableSpikeStrips)))
            {
                flags |= ERoadblockFlags.EnableSpikeStrips;
            }
            else
            {
                flags &= ~ERoadblockFlags.EnableSpikeStrips;
            }

            var actualLevelToUse = DetermineRoadblockLevelBasedOnTheRoadLocation(level, road);
            var roadblock = PursuitRoadblockFactory.Create(new PursuitRoadblockRequest
            {
                RoadblockData = GetRoadblockConfigDataForLevel(actualLevelToUse),
                Level = actualLevelToUse,
                Road = road,
                TargetVehicle = vehicle,
                TargetHeading = road.Heading,
                Flags = flags
            });

            _logger.Info($"Dispatching new roadblock as preview {createAsPreview}\n{roadblock}");
            lock (_roadblocks)
            {
                _roadblocks.Add(new RoadblockInfo(roadblock));
            }

            // subscribe to the roadblock events
            roadblock.RoadblockStateChanged += InternalRoadblockStateChanged;
            roadblock.RoadblockCopKilled += InternalRoadblockCopKilled;
            roadblock.RoadblockCopsJoiningPursuit += InternalRoadblockCopsJoiningThePursuit;

            if (createAsPreview)
            {
                roadblock.CreatePreview();
            }
            else
            {
                _logger.Trace($"Distance between vehicle and roadblock before spawn {road.Position.DistanceTo(vehicle.Position)}");
                var startedAt = DateTime.Now.Ticks;
                var result = roadblock.Spawn();
                var timeTakenToSpawn = (DateTime.Now.Ticks - startedAt) / TimeSpan.TicksPerMillisecond;
                if (!result)
                {
                    _logger.Warn($"Not all roadblock instances spawned with success for {roadblock}");
                }

                _game.DisplayNotificationDebug($"Roadblock spawned {ToColorText(result)}:~n~" +
                                               $"Distance: {road.Position.DistanceTo(vehicle.Position)}~n~" +
                                               $"Road width: {roadblock.Road.Width}~n~" +
                                               $"Slots: {roadblock.NumberOfSlots}~n~" +
                                               $"Spawn time: {timeTakenToSpawn} millis~n~" +
                                               $"Flags: {roadblock.Flags}");
                _logger.Trace($"Distance between vehicle and roadblock after spawn {road.Position.DistanceTo(vehicle.Position)}");
            }

            _logger.Info($"Roadblock has been dispatched, {roadblock}");
            return roadblock;
        }

        private bool IsRoadblockNearby(IVehicleNode street)
        {
            bool isThereANearbyRoadblock;

            lock (_roadblocks)
            {
                isThereANearbyRoadblock = _roadblocks
                    // filter out any previews and roadblocks in error state
                    // as we don't want them to prevent a roadblock placement
                    .Where(x => !x.Roadblock.IsPreviewActive && x.State != ERoadblockState.Error)
                    .Any(x => x.Position.DistanceTo(street.Position) <= MinimumDistanceBetweenRoadblocks);
            }

            return isThereANearbyRoadblock;
        }

        private IList<IVehicleNode> DetermineRoadblockLocation(ERoadblockLevel level, Vehicle vehicle, ERoadblockDistance roadblockDistance)
        {
            var distanceToUse = CalculateRoadblockDistance(vehicle, roadblockDistance);
            var roadType = DetermineAllowedRoadTypes(vehicle, level);

            _logger.Trace(
                $"Determining roadblock location for Position: {vehicle.Position}, Heading: {vehicle.Heading}, {nameof(distanceToUse)}: {distanceToUse}, {nameof(roadType)}: {roadType}");
            return RoadQuery.FindRoadsTraversing(vehicle.Position, vehicle.Heading, distanceToUse, roadType, DetermineBlacklistedFlagsForType(roadType))
                .ToList();
        }

        private ERoadblockLevel DetermineRoadblockLevelBasedOnTheRoadLocation(ERoadblockLevel level, Road street)
        {
            var actualLevelToUse = level;
            var isNonConcreteRoad = IsNonConcreteRoad(street);

            // check if big vehicles is not allowed on the node
            // if so reduce level 5 to level 4
            if (level == ERoadblockLevel.Level5 && street.Node.Flags.HasFlag(ENodeFlag.NoBigVehicles))
            {
                level = ERoadblockLevel.Level4;
                _logger.Debug($"Road disallows big vehicles, downgraded roadblock level to {level}");
            }

            // if we're not a dirt/offroad road
            // all levels are allowed
            _logger.Trace($"Roadblock placement is on dirt/offroad road: {isNonConcreteRoad}");
            if (!isNonConcreteRoad)
                return actualLevelToUse;

            // otherwise, we're going to reduce the level for simplification
            _logger.Debug("Detected a dirt/offroad position for the roadblock");
            if (level.Level > 3)
            {
                actualLevelToUse = ERoadblockLevel.Level2;
                _logger.Info($"Roadblock level has been reduced to {ERoadblockLevel.Level2} as the location is a dirt/offroad location");
            }

            return actualLevelToUse;
        }

        private void InternalRoadblockStateChanged(IRoadblock roadblock, ERoadblockState newState)
        {
            _logger.Debug($"Roadblock state changed to {newState}");

            _game.NewSafeFiber(() =>
            {
                if (newState == ERoadblockState.Disposed)
                {
                    _logger.Trace($"Removing roadblock {roadblock} from dispatcher");
                    RemoveRoadblock(roadblock);
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
                    .Where(x => !x.Roadblock.IsPreviewActive)
                    .Where(x => x.State is not (ERoadblockState.Active or ERoadblockState.Preparing or ERoadblockState.Disposing))
                    // verify if the player if far enough away for the roadblock to be cleaned
                    // if not, we auto clean roadblocks after AutoCleanRoadblockAfterSeconds
                    .Where(x => IsPlayerFarAwayFromRoadblock(x) || IsAutoRoadblockCleaningAllowed(x.Roadblock))
                    .ToList()
                    .ForEach(x =>
                    {
                        _game.DisplayNotificationDebug("~c~Roadblock is being disposed~n~" +
                                                       $"State: {x.State}~n~" +
                                                       $"Last state change: {_game.GameTime - x.Roadblock.LastStateChange}millis~n~" +
                                                       $"Distance from player: {x.Position.DistanceTo2D(_game.PlayerPosition)}");
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
            // #93: don't wait for this audio to complete as it sometimes causes too long delays between calculation and placement
            LspdfrUtils.PlayScannerAudio(AudioRequestConfirmed);
        }

        private void DenyUserRequestForRoadblock(bool userRequest, string reason)
        {
            _logger.Warn("Dispatching new roadblock is not allowed, " + reason);

            if (userRequest)
                LspdfrUtils.PlayScannerAudioNonBlocking(AudioRequestDenied);
        }

        private static bool IsNonConcreteRoad(Road street)
        {
            return street.Node.Flags.HasFlag(ENodeFlag.IsBackroad)
                   || street.Node.Flags.HasFlag(ENodeFlag.IsGravelRoad)
                   || street.Node.Flags.HasFlag(ENodeFlag.IsOffRoad);
        }

        private static float CalculateRoadblockDistance(Vehicle vehicle, ERoadblockDistance roadblockDistance)
        {
            return roadblockDistance switch
            {
                ERoadblockDistance.CurrentLocation => 5f,
                ERoadblockDistance.Closely => 50f,
                ERoadblockDistance.Default => DetermineRoadblockDistanceFor(vehicle),
                ERoadblockDistance.Far => 250f,
                ERoadblockDistance.VeryFar => 500f,
                ERoadblockDistance.ExtremelyFar => 1000f,
                _ => throw new ArgumentOutOfRangeException(nameof(roadblockDistance), roadblockDistance, "roadblock distance is not supported")
            };
        }

        private static float DetermineRoadblockDistanceFor(Vehicle vehicle)
        {
            var vehicleSpeed = vehicle.Speed;
            var distance = vehicleSpeed * 4f;

            if (distance < MinimumRoadblockPlacementDistance)
                distance = MinimumRoadblockPlacementDistance;

            return distance;
        }

        private EVehicleNodeType DetermineAllowedRoadTypes(Vehicle vehicle, ERoadblockLevel level)
        {
            // verify the current road type
            // if we're already at a dirt/offroad road, all road types for the trajectory calculation are allowed
            if (RoadQuery.IsSlowRoad(vehicle.Position))
            {
                _logger.Debug("Following the current dirt/offroad road for the roadblock placement");
                return EVehicleNodeType.AllNodes;
            }

            // otherwise, we're going to base the allowed road types for the trajectory based
            // on the current roadblock level
            var vehicleNodeType = level.Level <= ERoadblockLevel.Level2.Level ? EVehicleNodeType.AllNodes : EVehicleNodeType.MainRoadsWithJunctions;
            _logger.Debug($"Roadblock road traversal will use vehicle node type {vehicleNodeType}");
            return vehicleNodeType;
        }

        private ENodeFlag DetermineBlacklistedFlagsForType(EVehicleNodeType vehicleNodeType)
        {
            var flags = ENodeFlag.IsOnWater;

            if (vehicleNodeType == EVehicleNodeType.MainRoads)
            {
                flags |= ENodeFlag.IsBackroad | ENodeFlag.IsGravelRoad | ENodeFlag.IsOffRoad;
            }

            _logger.Debug($"Following node flags are blacklisted for determining the location {flags}");
            return flags;
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

        private RoadblockData GetRoadblockConfigDataForLevel(ERoadblockLevel level)
        {
            var data = _roadblockData.Roadblocks.Items.FirstOrDefault(x => x.Level == level.Level);
            if (data == null)
                throw new RoadblockException($"Roadblock data couldn't be found for level {level}");

            return data;
        }

        /// <summary>
        /// Filter out any roads of the intersection which travel in the same heading as the target.
        /// </summary>
        private IEnumerable<Road> FilterRoadsTravellingAlongTheRoute([CanBeNull] IVehicleNode previousNode, Intersection intersection)
        {
            _logger.Debug($"Filtering applicable intersection roads for intersection {intersection}");
            var headingToVerify = previousNode?.Heading ?? intersection.Heading;
            return intersection.Roads
                .Where(road => Math.Abs(road.Heading - headingToVerify) > 35f && Math.Abs(road.Heading - headingToVerify) < 170f)
                .Where(x => (x.Node.Flags & (ENodeFlag.IsAlley | ENodeFlag.IsGravelRoad)) == 0);
        }

        private static IVehicleNode GetPreviousNode(IList<IVehicleNode> streetNodes, IVehicleNode currentNode)
        {
            var previousNodeIndex = streetNodes.IndexOf(currentNode) - 1;

            return previousNodeIndex >= 0 ? streetNodes[previousNodeIndex] : null;
        }

        private static string ToColorText(bool result)
        {
            var color = result ? "g" : "r";
            return $"~{color}~{result}~s~";
        }

        #endregion
    }
}