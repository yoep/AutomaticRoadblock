using System;
using System.Diagnostics;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Dispatcher;
using AutomaticRoadblocks.Settings;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AutomaticRoadblocks.Pursuit
{
    public class PursuitManager : IPursuitManager
    {
        private readonly ILogger _logger;
        private readonly IGame _game;
        private readonly ISettingsManager _settingsManager;
        private readonly IRoadblockDispatcher _roadblockDispatcher;
        private readonly Random _random = new();

        private int _totalCopsKilled;
        private int _totalRoadblocksDeployed;
        private uint _timePursuitStarted;
        private uint _timeLastDispatchedRoadblock;
        private uint _timeLastRoadblockActive;
        private uint _timeLastLevelChanged;
        private PursuitLevel _pursuitLevel = PursuitLevel.Level1;

        public PursuitManager(ILogger logger, IGame game, ISettingsManager settingsManager, IRoadblockDispatcher roadblockDispatcher)
        {
            _logger = logger;
            _game = game;
            _settingsManager = settingsManager;
            _roadblockDispatcher = roadblockDispatcher;
        }

        #region Properties

        /// <inheritdoc />
        public LHandle PursuitHandle { get; private set; }

        /// <inheritdoc />
        public bool EnableAutomaticDispatching { get; set; }

        /// <inheritdoc />
        public bool IsPursuitActive => PursuitHandle != null && Functions.IsPursuitStillRunning(PursuitHandle);

        /// <inheritdoc />
        public bool IsPursuitOnFoot
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
        }

        public void StopListener()
        {
            _logger.Trace("Removing pursuit listener from LSPD_First_Response Api");
            Events.OnPursuitStarted -= PursuitStarted;
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
        public bool DispatchNow(bool force = false)
        {
            if (!IsPursuitActive)
            {
                return DispatchForNonActivePursuit(force);
            }

            var vehicle = GetSuspectVehicle();

            // try to dispatch a new roadblock for a chased vehicle
            // if successful, store the dispatch time
            return vehicle != null && DoDispatch(vehicle, force);
        }

        /// <inheritdoc />
        public void DispatchPreview(bool currentLocation)
        {
            var vehicle = GetSuspectVehicle() ?? _game.PlayerVehicle;

            if (vehicle == null)
            {
                _logger.Warn("Unable to create pursuit roadblock preview, no active vehicle in pursuit or player not in vehicle");
                return;
            }

            _logger.Trace("Creating pursuit roadblock preview");
            _roadblockDispatcher.DispatchPreview(ToRoadblockLevel(PursuitLevel), vehicle, currentLocation);
        }

        #endregion

        #region Functions

        private void UpdatePursuitLevel(PursuitLevel level)
        {
            Assert.NotNull(level, "level cannot be null");
            _logger.Debug($"Updating pursuit level to {level}");
            PursuitLevel = level;
            PursuitLevelChanged?.Invoke(level);
            NotifyPursuitLevelIncreased();
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
            _roadblockDispatcher.RoadblockCopKilled += RoadblockCopKilled;
            _roadblockDispatcher.RoadblockStateChanged += RoadblockStateChanged;
            PursuitHandle = pursuitHandle;
            UpdatePursuitLevel(PursuitLevel.Level1);

            StartPursuitMonitor();

            PursuitStateChanged?.Invoke(true);
        }

        private void PursuitEnded(LHandle handle)
        {
            PursuitHandle = null;
            _roadblockDispatcher.RoadblockCopKilled -= RoadblockCopKilled;

            PursuitStateChanged?.Invoke(false);
        }

        private void StartPursuitMonitor()
        {
            _game.NewSafeFiber(() =>
            {
                while (IsPursuitActive)
                {
                    DoAutoLevelIncrementTick();
                    DoLevelIncreaseTick();
                    DoDispatchTick();

                    _game.FiberYield();
                }
            }, "PursuitManager.PursuitMonitor");
        }

        private void DoAutoLevelIncrementTick()
        {
            if (!_settingsManager.AutomaticRoadblocksSettings.EnableAutoLevelIncrements)
                return;

            VerifyPursuitLethalForce();
            VerifyShotsFired();
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
            if (Functions.GetPursuitPeds(PursuitHandle).Any(x => x.IsShooting || x.IsAiming) && PursuitLevel.Level < 2)
            {
                _logger.Debug("Suspect is shooting/aiming, increasing level");
                UpdatePursuitLevel(PursuitLevel.Level2);
            }

            switch (PursuitLevel.Level)
            {
                case < 2 when _totalCopsKilled > 0:
                    _logger.Debug("Suspect has killed a roadblock cop, increasing level");
                    UpdatePursuitLevel(PursuitLevel.Level2);
                    break;
                case < 3 when _totalCopsKilled >= 3:
                    _logger.Debug("Suspect has killed multiple roadblock cops, increasing level");
                    UpdatePursuitLevel(PursuitLevel.Level3);
                    break;
            }
        }

        private void DoLevelIncreaseTick()
        {
            
            var increaseLevelThreshold = 100 - PursuitLevel.AutomaticLevelIncreaseFactor * 100;

            if (HasAtLeastOneRoadblockDeployed() &&
                HasEnoughTimePassedBetweenLastLevelIncrease() &&
                _random.Next(101) >= increaseLevelThreshold)
                IncreasePursuitLevel();
        }

        private bool DoDispatch(Vehicle vehicle, bool force)
        {
            var dispatched = _roadblockDispatcher.Dispatch(ToRoadblockLevel(PursuitLevel), vehicle, force);

            if (dispatched)
            {
                _timeLastDispatchedRoadblock = _game.GameTime;
                _totalRoadblocksDeployed++;
            }

            return dispatched;
        }

        private void DoDispatchTick()
        {
            if (IsDispatchingAllowed() && ShouldDispatchRoadblock())
                DispatchNow();
        }

        private bool DispatchForNonActivePursuit(bool force)
        {
            if (force)
                return DoDispatch(_game.PlayerVehicle, true);

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
                   Functions.GetPursuitPeds(PursuitHandle).Any(x => !Functions.IsPedVisualLost(x));
        }

        private void RoadblockStateChanged(IRoadblock roadblock, RoadblockState newState)
        {
            if (newState == RoadblockState.Active)
                _timeLastRoadblockActive = _game.GameTime;
        }

        private void RoadblockCopKilled(IRoadblock roadblock)
        {
            _totalCopsKilled++;
        }

        private bool HasAtLeastOneRoadblockDeployed()
        {
            return _totalRoadblocksDeployed > 0;
        }

        private bool HasEnoughTimePassedBetweenLastLevelIncrease()
        {
            var gameTime = _game.GameTime;
            
            return gameTime - _timeLastLevelChanged > _settingsManager.AutomaticRoadblocksSettings.TimeBetweenAutoLevelIncrements * 1000;
        }

        [Conditional("DEBUG")]
        private void NotifyPursuitLevelIncreased()
        {
            _game.DisplayNotification($"Pursuit level has changed to ~g~{PursuitLevel.Level}");
        }

        private static RoadblockLevel ToRoadblockLevel(PursuitLevel pursuitLevel)
        {
            return RoadblockLevel.Levels
                .First(x => x.Level == pursuitLevel.Level);
        }

        #endregion
    }
}