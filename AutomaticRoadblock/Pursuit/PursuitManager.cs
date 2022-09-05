using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Dispatcher;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AutomaticRoadblocks.Pursuit
{
    public class PursuitManager : IPursuitManager
    {
        private const int TimeBetweenLevelIncreaseShotsFired = 20 * 1000;
        private const int TimeAfterLastDeploymentBeforeIncreasingLevel = 10 * 1000;

        private readonly ILogger _logger;
        private readonly IGame _game;
        private readonly ISettingsManager _settingsManager;
        private readonly IRoadblockDispatcher _roadblockDispatcher;
        private readonly ILocalizer _localizer;
        private readonly Random _random = new();

        private int _totalCopsKilled;
        private int _totalRoadblocksDeployed;
        private uint _timePursuitStarted;
        private uint _timeLastDispatchedRoadblock;
        private uint _timeLastRoadblockActive;
        private uint _timeLastLevelChanged;
        private uint _timeLastLevelChangedForShotsFired;
        private PursuitLevel _pursuitLevel = PursuitLevel.Level1;
        private bool _keyListenerActive;

        public PursuitManager(ILogger logger, IGame game, ISettingsManager settingsManager, IRoadblockDispatcher roadblockDispatcher, ILocalizer localizer)
        {
            _logger = logger;
            _game = game;
            _settingsManager = settingsManager;
            _roadblockDispatcher = roadblockDispatcher;
            _localizer = localizer;
            EnableAutomaticDispatching = settingsManager.AutomaticRoadblocksSettings.EnableDuringPursuits;
            EnableAutomaticLevelIncreases = true;
            EnableSpikeStrips = settingsManager.AutomaticRoadblocksSettings.EnableSpikeStrips;
        }

        #region Properties

        /// <summary>
        /// The pursuit handle of the currently active pursuit.
        /// When <see cref="IsPursuitActive"/> returns false, this property will be null.
        /// </summary>
        public LHandle PursuitHandle { get; private set; }

        /// <inheritdoc />
        public bool EnableAutomaticDispatching { get; set; }

        /// <inheritdoc />
        public bool EnableAutomaticLevelIncreases { get; set; }

        /// <inheritdoc />
        public bool EnableSpikeStrips { get; set; }

        /// <inheritdoc />
        public bool IsPursuitActive => PursuitHandle != null && Functions.IsPursuitStillRunning(PursuitHandle);

        /// <inheritdoc />
        public EPursuitState State { get; private set; } = EPursuitState.Inactive;

        /// <inheritdoc />
        public PursuitLevel PursuitLevel
        {
            get => _pursuitLevel;
            set
            {
                _pursuitLevel = value;
                _timeLastLevelChanged = _game.GameTime;
                _logger.Debug($"Pursuit level has changed to {value}");
            }
        }

        /// <summary>
        /// Verify if the pursuit is on foot and not anymore in vehicles.
        /// This means that all of the suspects are not in a vehicle anymore.
        /// </summary>
        /// <exception cref="NoPursuitActiveException">Is thrown when this property is called and <see cref="IsPursuitActive"/> is false.</exception>
        private bool IsPursuitOnFoot
        {
            get
            {
                if (!IsPursuitActive)
                {
                    throw new NoPursuitActiveException();
                }

                return Functions.GetPursuitPeds(PursuitHandle)
                    .All(x => x.CurrentVehicle == null);
            }
        }

        /// <summary>
        /// Verify if the visual on all suspects has been lost.
        /// </summary>
        /// <exception cref="NoPursuitActiveException">Is thrown when this property is called and <see cref="IsPursuitActive"/> is false.</exception>
        private bool IsPursuitVisualLost
        {
            get
            {
                if (!IsPursuitActive)
                {
                    throw new NoPursuitActiveException();
                }

                return Functions.GetPursuitPeds(PursuitHandle)
                    .All(Functions.IsPedVisualLost);
            }
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public event PursuitEvents.PursuitStateChangedEventHandler PursuitStateChanged;

        /// <inheritdoc />
        public event PursuitEvents.PursuitLevelChangedEventHandler PursuitLevelChanged;

        #endregion

        #region Methods

        public void StartListener()
        {
            _logger.Trace("Registering pursuit listener to LSPD_First_Response Api");
            Events.OnPursuitStarted += PursuitStarted;
            Events.OnPursuitEnded += PursuitEnded;
            StartKeyListener();
        }

        public void StopListener()
        {
            _logger.Trace("Removing pursuit listener from LSPD_First_Response Api");
            Events.OnPursuitStarted -= PursuitStarted;
            Events.OnPursuitEnded -= PursuitEnded;
            _keyListenerActive = false;
        }

        /// <inheritdoc />
        public Vehicle GetSuspectVehicle()
        {
            if (!IsPursuitActive)
                return null;

            return Functions.GetPursuitPeds(PursuitHandle)
                .Where(x => !Functions.IsPedVisualLost(x))
                .Select(x => x.CurrentVehicle)
                .FirstOrDefault(x => x != null);
        }

        /// <inheritdoc />
        public bool DispatchNow(bool userRequested = false, bool force = false, bool atCurrentLocation = false)
        {
            if (!IsPursuitActive)
            {
                return DispatchForNonActivePursuit(userRequested, force, atCurrentLocation);
            }

            var vehicle = GetSuspectVehicle();

            // try to dispatch a new roadblock for a chased vehicle
            // if successful, store the dispatch time
            return vehicle != null && DoDispatch(vehicle, userRequested, force, false);
        }

        /// <inheritdoc />
        public void DispatchPreview(bool currentLocation)
        {
            var vehicle = GetSuspectVehicle() ?? _game.PlayerVehicle;

            if (vehicle == null)
            {
                _logger.Warn("Unable to create pursuit roadblock preview, no active vehicle in pursuit or player not in vehicle");
                _game.DisplayNotification(_localizer[LocalizationKey.RoadblockNoPursuitActive]);
                return;
            }

            _logger.Trace("Creating pursuit roadblock preview");
            _roadblockDispatcher.DispatchPreview(ToRoadblockLevel(PursuitLevel), vehicle, new DispatchOptions
            {
                EnableSpikeStrips = EnableSpikeStrips,
                AtCurrentLocation = currentLocation
            });
        }

        #endregion

        #region Functions

        private bool IsDispatchNowPressed()
        {
            var roadblocksSettings = _settingsManager.AutomaticRoadblocksSettings;
            var secondKey = roadblocksSettings.DispatchNowModifierKey;
            var secondKeyDown = secondKey == Keys.None;

            if (!secondKeyDown && Game.IsKeyDownRightNow(secondKey))
                secondKeyDown = true;

            return Game.IsKeyDown(roadblocksSettings.DispatchNowKey) && secondKeyDown;
        }

        private void UpdatePursuitLevel(PursuitLevel level)
        {
            Assert.NotNull(level, "level cannot be null");
            _logger.Debug($"Updating pursuit level to {level}");
            PursuitLevel = level;
            PursuitLevelChanged?.Invoke(level);
            NotifyPursuitLevelIncreased();

            if (level.Level > 1)
                LspdfrUtils.PlayScannerAudioNonBlocking(level.AudioFile);
        }

        private void IncreasePursuitLevel()
        {
            var newLevel = PursuitLevel.Level + 1;

            // check that the max level hasn't already been reached
            if (newLevel > PursuitLevel.Level5.Level)
                return;

            UpdatePursuitLevel(PursuitLevel.From(newLevel));
        }

        private void PursuitStarted(LHandle pursuitHandle)
        {
            _logger.Trace("Pursuit has been started");
            _totalCopsKilled = 0;
            _totalRoadblocksDeployed = 0;
            _timePursuitStarted = _game.GameTime;
            _timeLastLevelChanged = _game.GameTime;
            _timeLastLevelChangedForShotsFired = 0;
            _roadblockDispatcher.RoadblockCopKilled += RoadblockCopKilled;
            _roadblockDispatcher.RoadblockStateChanged += RoadblockStateChanged;
            _roadblockDispatcher.RoadblockCopsJoiningPursuit += RoadblockCopsJoiningPursuit;
            PursuitHandle = pursuitHandle;

            // check in which state the pursuit is started
            UpdateState(IsPursuitOnFoot ? EPursuitState.ActiveOnFoot : EPursuitState.ActiveChase);

            // only reset the pursuit level if automatic level increases is enabled
            // otherwise, leave it at the current level
            if (EnableAutomaticLevelIncreases)
                UpdatePursuitLevel(PursuitLevel.Level1);

            StartPursuitMonitor();

            PursuitStateChanged?.Invoke(true);
        }

        private void PursuitEnded(LHandle handle)
        {
            _logger.Debug("Pursuit has ended, cleaning up any remaining pursuit roadblocks");
            PursuitHandle = null;
            UpdateState(EPursuitState.Inactive);
            _roadblockDispatcher.RoadblockCopKilled -= RoadblockCopKilled;
            _roadblockDispatcher.DismissActiveRoadblocks();

            PursuitStateChanged?.Invoke(false);
        }

        private void StartPursuitMonitor()
        {
            _game.NewSafeFiber(() =>
            {
                while (IsPursuitActive)
                {
                    DoStateVerificationTick();
                    DoAutoLevelIncrementTick();
                    DoDispatchTick();

                    _game.FiberYield();
                }
            }, "PursuitManager.PursuitMonitor");
        }

        private void DoStateVerificationTick()
        {
            if (!IsPursuitActive)
            {
                UpdateState(EPursuitState.Inactive);
            }
            else if (IsPursuitOnFoot)
            {
                UpdateState(EPursuitState.ActiveOnFoot);
            }
            else if (IsPursuitVisualLost)
            {
                UpdateState(EPursuitState.ActiveVisualLost);
            }
            else
            {
                UpdateState(EPursuitState.ActiveChase);
            }
        }

        private void DoAutoLevelIncrementTick()
        {
            // verify if automatic level increases is disabled
            // or the pursuit is not anymore in the preferred state
            // if so, skip the condition checking
            if (!EnableAutomaticLevelIncreases || State != EPursuitState.ActiveChase)
                return;

            VerifyPursuitLethalForce();
            VerifyShotsFired();
            DoLevelIncreaseTick();
        }

        private void VerifyPursuitLethalForce()
        {
            if (IsPursuitActive && !Functions.IsPursuitLethalForceForced(PursuitHandle))
                return;

            var firstLethalLevel = PursuitLevel.Levels
                .First(x => x.IsLethalForceAllowed);

            // make sure that the current level is at least a lethal level
            // if not, update the current level to the first lethal level
            if (PursuitLevel.Level < firstLethalLevel.Level)
            {
                UpdatePursuitLevel(firstLethalLevel);
            }
        }

        private void VerifyShotsFired()
        {
            if (!IsPursuitActive)
                return;

            if (IsAnySuspectAimingOrShooting() && IsAutomaticLevelIncreaseForShotsFiredAllowed())
            {
                _logger.Debug("Suspect is shooting/aiming, increasing level");
                _timeLastLevelChangedForShotsFired = _game.GameTime;
                UpdatePursuitLevel(PursuitLevel.From(PursuitLevel.Level + 1));
            }

            switch (PursuitLevel.Level)
            {
                case < 2 when _totalCopsKilled > 0:
                    _logger.Debug("Suspect has killed a roadblock cop, increasing level");
                    UpdatePursuitLevel(PursuitLevel.Level2);
                    break;
                case < 3 when _totalCopsKilled >= 2:
                    _logger.Debug("Suspect has killed multiple roadblock cops, increasing level");
                    UpdatePursuitLevel(PursuitLevel.Level3);
                    break;
            }
        }

        private void DoLevelIncreaseTick()
        {
            var increaseLevelThreshold = 100 - PursuitLevel.AutomaticLevelIncreaseFactor * 100;

            if (IsAutomaticLevelIncreaseAllowed() &&
                _random.Next(101) >= increaseLevelThreshold)
            {
                IncreasePursuitLevel();
            }
        }

        private bool IsAnySuspectAimingOrShooting()
        {
            return Functions.GetPursuitPeds(PursuitHandle).Any(x => x.IsShooting || x.IsAiming);
        }

        private bool IsAutomaticLevelIncreaseForShotsFiredAllowed()
        {
            var allowedSinceLastLevelChange = _game.GameTime - _timeLastLevelChangedForShotsFired > TimeBetweenLevelIncreaseShotsFired;

            return (PursuitLevel.Level < 2 && allowedSinceLastLevelChange) ||
                   (PursuitLevel.Level == 2 && _totalRoadblocksDeployed > 0 && allowedSinceLastLevelChange) ||
                   (PursuitLevel.Level < 3 && _totalRoadblocksDeployed > 1 && allowedSinceLastLevelChange);
        }

        private bool IsAutomaticLevelIncreaseAllowed()
        {
            return HasAtLeastDeployedXRoadblocks() &&
                   HasEnoughTimePassedBetweenLastLevelIncrease() &&
                   HasEnoughTimePassedAfterLastDeployment();
        }

        private bool DoDispatch(Vehicle vehicle, bool userRequested, bool force, bool atCurrentLocation)
        {
            _logger.Debug(
                $"Dispatching roadblock for pursuit with {nameof(PursuitLevel)}: {PursuitLevel}, {nameof(userRequested)}: {userRequested}, " +
                $"{nameof(force)}: {force}, {nameof(atCurrentLocation)}: {atCurrentLocation}");
            var roadblock = _roadblockDispatcher.Dispatch(ToRoadblockLevel(PursuitLevel), vehicle, new DispatchOptions
            {
                EnableSpikeStrips = EnableSpikeStrips,
                IsUserRequested = userRequested,
                Force = force,
                AtCurrentLocation = atCurrentLocation
            });

            if (roadblock == null)
            {
                _logger.Warn("Pursuit roadblock was not dispatched");
                return false;
            }

            _timeLastDispatchedRoadblock = _game.GameTime;
            _totalRoadblocksDeployed++;
            _logger.Info("Pursuit roadblock has been dispatched");
            return true;
        }

        private void DoDispatchTick()
        {
            if (IsDispatchingAllowed() && ShouldDispatchRoadblock())
                DispatchNow();
        }

        private bool DispatchForNonActivePursuit(bool userRequest, bool force, bool atCurrentLocation)
        {
            if (!userRequest && force)
                return DoDispatch(_game.PlayerVehicle, false, true, atCurrentLocation);

            _logger.Warn("Unable to dispatch roadblock, no active pursuit ongoing");
            return false;
        }

        private bool ShouldDispatchRoadblock()
        {
            if (!IsPursuitActive)
                return false;

            return _random.Next(0, 101) >= 100 - PursuitLevel.DispatchFactor * 100;
        }

        private bool IsDispatchingAllowed()
        {
            if (!IsPursuitActive)
                return false;

            var roadblocksSettings = _settingsManager.AutomaticRoadblocksSettings;
            var gameTime = _game.GameTime;

            return gameTime - _timePursuitStarted >= roadblocksSettings.DispatchAllowedAfter * 1000 &&
                   gameTime - _timeLastDispatchedRoadblock >= roadblocksSettings.DispatchInterval * 1000 &&
                   gameTime - _timeLastRoadblockActive >= roadblocksSettings.DispatchInterval * 1000 &&
                   IsSuspectVehicleOnRoad() &&
                   Functions.GetPursuitPeds(PursuitHandle).Any(x => !Functions.IsPedVisualLost(x));
        }

        private void RoadblockStateChanged(IRoadblock roadblock, ERoadblockState newState)
        {
            if (newState == ERoadblockState.Active)
                _timeLastRoadblockActive = _game.GameTime;
        }

        private void RoadblockCopKilled(IRoadblock roadblock)
        {
            _totalCopsKilled++;
            NotifySuspectKilledCop();
            LspdfrUtils.PlayScannerAudio("ROADBLOCK_COP_KILLED");
        }

        private void RoadblockCopsJoiningPursuit(IRoadblock roadblock, IEnumerable<Ped> cops)
        {
            if (!IsPursuitActive)
                return;

            cops.ToList().ForEach(x => Functions.AddCopToPursuit(PursuitHandle, x));
        }

        private bool IsSuspectVehicleOnRoad()
        {
            var vehicle = GetSuspectVehicle();

            return vehicle != null && RoadUtils.IsPointOnRoad(vehicle.Position);
        }

        private bool HasAtLeastDeployedXRoadblocks()
        {
            return _totalRoadblocksDeployed > 1;
        }

        private bool HasEnoughTimePassedBetweenLastLevelIncrease()
        {
            return _game.GameTime - _timeLastLevelChanged > _settingsManager.AutomaticRoadblocksSettings.TimeBetweenAutoLevelIncrements * 1000;
        }

        private bool HasEnoughTimePassedAfterLastDeployment()
        {
            // this check should prevent the level from being increased directly after a roadblock has been spawned
            return _game.GameTime - _timeLastDispatchedRoadblock > TimeAfterLastDeploymentBeforeIncreasingLevel;
        }

        private void OnStateChanged()
        {
            // verify if the state changed to on foot
            // if not, ignore the state change as nothing is applied
            if (State != EPursuitState.ActiveOnFoot)
                return;

            _roadblockDispatcher.DismissActiveRoadblocks();
        }

        private void StartKeyListener()
        {
            _keyListenerActive = true;
            _game.NewSafeFiber(() =>
            {
                _logger.Debug("Pursuit manager key listener has been started");
                while (_keyListenerActive)
                {
                    _game.FiberYield();

                    try
                    {
                        if (!IsPursuitActive || !IsDispatchNowPressed())
                            continue;

                        _logger.Debug("User pressed the dispatch now key, trying to dispatch a new roadblock");
                        DispatchNow(true);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"An error occurred while processing the pursuit manager key listener, {ex.Message}", ex);
                    }
                }

                _logger.Debug("Pursuit manager key listener has been stopped");
            }, "PursuitManager.KeyListener");
        }

        private void UpdateState(EPursuitState newState)
        {
            if (State == newState)
                return;

            State = newState;
            _logger.Info($"Pursuit state has changed to {State}");
            OnStateChanged();
        }

        [Conditional("DEBUG")]
        private void NotifyPursuitLevelIncreased()
        {
            _game.DisplayNotification($"Pursuit level has changed to ~g~{PursuitLevel.Level}");
        }

        [Conditional("DEBUG")]
        private void NotifySuspectKilledCop()
        {
            _game.DisplayNotification($"Suspect killed a total of ~r~{_totalCopsKilled}~s~ cops");
        }

        private static RoadblockLevel ToRoadblockLevel(PursuitLevel pursuitLevel)
        {
            return RoadblockLevel.Levels
                .First(x => x.Level == pursuitLevel.Level);
        }

        #endregion
    }
}