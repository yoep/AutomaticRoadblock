using System;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Settings;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AutomaticRoadblocks.Pursuit
{
    public class PursuitManager : IPursuitManager
    {
        private const double NonLethalDispatchFactor = 0.15;
        private const double LethalDispatchFactor = 0.25;

        private readonly ILogger _logger;
        private readonly IGame _game;
        private readonly ISettingsManager _settingsManager;
        private readonly IRoadblockDispatcher _roadblockDispatcher;
        private readonly Random _random = new Random();

        private uint _timePursuitStarted;
        private uint _timeLastDispatchedRoadblock;

        public PursuitManager(ILogger logger, IGame game, ISettingsManager settingsManager, IRoadblockDispatcher roadblockDispatcher)
        {
            _logger = logger;
            _game = game;
            _settingsManager = settingsManager;
            _roadblockDispatcher = roadblockDispatcher;
            PursuitLevel = PursuitLevel.Level1;
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
        public PursuitLevel PursuitLevel { get; private set; }

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
                .Select(x => x.CurrentVehicle)
                .First(x => x != null);
        }

        /// <inheritdoc />
        public bool DispatchNow()
        {
            if (!IsPursuitActive)
            {
                _logger.Warn("Unable to dispatch roadblock, no active pursuit ongoing");
                return false;
            }

            var vehicle = GetSuspectVehicle();

            // try to dispatch a new roadblock for the chased vehicle
            // if successful, store the dispatch time
            if (!_roadblockDispatcher.Dispatch(ToRoadblockLevel(PursuitLevel), vehicle))
                return false;

            _timeLastDispatchedRoadblock = _game.GameTime;
            return true;
        }

        /// <inheritdoc />
        public void DispatchPreview()
        {
            var vehicle = GetSuspectVehicle() ?? _game.PlayerVehicle;

            if (vehicle == null)
            {
                _logger.Warn("Unable to create pursuit roadblock preview, no active vehicle in pursuit or player not in vehicle");
                return;
            }

            _logger.Trace("Creating pursuit roadblock preview");
            _roadblockDispatcher.DispatchPreview(ToRoadblockLevel(PursuitLevel), vehicle);
        }

        /// <inheritdoc />
        public void UpdatePursuitLevel(PursuitLevel level)
        {
            Assert.NotNull(level, "level cannot be null");
            _logger.Debug($"Updating pursuit level to {PursuitLevel.Level3}");
            PursuitLevel = level;
            PursuitLevelChanged?.Invoke(level);
        }

        #endregion

        #region Functions

        private void PursuitStarted(LHandle pursuitHandle)
        {
            _logger.Trace("Pursuit has been started");
            _timePursuitStarted = _game.GameTime;
            PursuitHandle = pursuitHandle;
            UpdatePursuitLevel(PursuitLevel.Level1);

            StartPursuitMonitor();

            PursuitStateChanged?.Invoke(true);
        }

        private void PursuitEnded(LHandle handle)
        {
            PursuitHandle = null;

            PursuitStateChanged?.Invoke(false);
        }

        private void StartPursuitMonitor()
        {
            _game.NewSafeFiber(() =>
            {
                while (IsPursuitActive)
                {
                    _game.FiberYield();

                    VerifyPursuitLethalForce();
                    DoDispatchTick();
                }
            }, "PursuitManager.PursuitMonitor");
        }

        private void VerifyPursuitLethalForce()
        {
            if (IsPursuitActive && !Functions.IsPursuitLethalForceForced(PursuitHandle))
                return;

            var firstLethalLevel = PursuitLevel.Levels
                .First(x => x.LethalAllowed);

            // make that the current level is at least a lethal level
            // if not, update the current level to the first lethal level
            if (PursuitLevel.Level < firstLethalLevel.Level)
            {
                UpdatePursuitLevel(firstLethalLevel);
            }
        }

        private void DoDispatchTick()
        {
            if (IsDispatchingAllowed() && ShouldDispatchRoadblock())
                DispatchNow();
        }

        private bool ShouldDispatchRoadblock()
        {
            if (!IsPursuitActive)
                return false;

            var possibility = Functions.IsPursuitLethalForceForced(PursuitHandle) ? LethalDispatchFactor : NonLethalDispatchFactor;
            var dispatchThreshold = 100 - possibility * 100;

            return _random.Next(0, 101) >= dispatchThreshold;
        }

        private bool IsDispatchingAllowed()
        {
            var roadblocksSettings = _settingsManager.AutomaticRoadblocksSettings;
            var gameTime = _game.GameTime;

            return gameTime - _timePursuitStarted >= roadblocksSettings.DispatchAllowedAfter * 1000 &&
                   gameTime - _timeLastDispatchedRoadblock >= roadblocksSettings.DispatchInterval * 1000;
        }

        private static RoadblockLevel ToRoadblockLevel(PursuitLevel pursuitLevel)
        {
            return RoadblockLevel.Levels
                .First(x => x.Level == pursuitLevel.Level);
        }

        #endregion
    }
}