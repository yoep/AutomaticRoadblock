using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    public class Roadblock : IRoadblock
    {
        private const float LaneHeadingTolerance = 40f;
        private const float BypassTolerance = 20f;
        private const float SpeedLimit = 5f;

        private readonly ILogger _logger;
        private readonly Road _road;
        private readonly Vehicle _vehicle;
        private readonly List<RoadblockSlot> _slots = new List<RoadblockSlot>();

        private Blip _blip;
        private int _speedZoneId;
        private float _lastKnownDistanceToRoadblock = 9999f;

        internal Roadblock(RoadblockLevel level, Road road, Vehicle vehicle, bool limitSpeed)
        {
            Assert.NotNull(level, "level cannot be null");
            Assert.NotNull(road, "road cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");
            _logger = IoC.Instance.GetInstance<ILogger>();
            _road = road;
            _vehicle = vehicle;
            Level = level;

            Init(limitSpeed);
        }

        #region Properties

        /// <summary>
        /// Get the level of the roadblock.
        /// </summary>
        public RoadblockLevel Level { get; }

        /// <summary>
        /// Get the state of the roadblock.
        /// </summary>
        public RoadblockState State { get; private set; } = RoadblockState.Preparing;

        /// <summary>
        /// Get the central position of the roadblock.
        /// </summary>
        public Vector3 Postion => _road.Position;

        #endregion

        #region Events

        /// <summary>
        /// Invoked when the roadblock state changes.
        /// </summary>
        public event RoadblockEvents.RoadblockStateChanged RoadblockStateChanged;

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => _slots
            .Select(x => x.IsPreviewActive)
            .FirstOrDefault();

        /// <inheritdoc />
        public void CreatePreview()
        {
            CreateBlip();
            foreach (var roadblockSlot in _slots)
            {
                roadblockSlot.CreatePreview();
            }
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            DeleteBlip();
            foreach (var roadblockSlot in _slots)
            {
                roadblockSlot.DeletePreview();
            }
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            if (IsPreviewActive)
                DeletePreview();

            DeleteSpeedZone();
            _slots.ForEach(x => x.Dispose());
        }

        #endregion

        /// <summary>
        /// Spawn the roadblock in the world.
        /// </summary>
        public void Spawn()
        {
            try
            {
                _logger.Trace("Spawning roadblock");
                UpdateState(RoadblockState.Active);

                foreach (var slot in _slots)
                {
                    slot.Spawn();
                }

                CreateBlip();
                Monitor();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to spawn roadblock", ex);
                UpdateState(RoadblockState.Error);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Level)}: {Level}\n" +
                   $"{nameof(_slots)}: {_slots}\n" +
                   $"{nameof(State)}: {State}" +
                   $"{nameof(_road)}: {_road}";
        }

        private void Init(bool limitSpeed)
        {
            var lanesToBlock = _road.Lanes
                .Where(x => Math.Abs(x.Heading - _vehicle.Heading) < LaneHeadingTolerance)
                .ToList();
            _logger.Debug($"Roadblock will block {lanesToBlock.Count} lanes");

            foreach (var lane in lanesToBlock)
            {
                _slots.Add(new RoadblockSlot(lane.Position, lane.Heading));
            }

            if (limitSpeed)
            {
                _logger.Trace("Creating speed zone at roadblock");
                CreateSpeedZoneLimit();
            }
        }

        private void CreateSpeedZoneLimit()
        {
            try
            {
                _speedZoneId = RoadUtils.CreateSpeedZone(Postion, 10f, SpeedLimit);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to create roadblock speed zone", ex);
            }
        }

        private void DeleteSpeedZone()
        {
            if (_speedZoneId != 0)
                RoadUtils.RemoveSpeedZone(_speedZoneId);
        }

        private void CreateBlip()
        {
            if (_blip != null)
                return;

            _blip = new Blip(Postion)
            {
                IsRouteEnabled = false,
                IsFriendly = true,
                Scale = 1f,
                Color = Color.Green
            };
        }

        private void DeleteBlip()
        {
            if (_blip != null)
                _blip.Delete();
        }

        private void Monitor()
        {
            var game = IoC.Instance.GetInstance<IGame>();

            game.NewSafeFiber(() =>
            {
                while (State == RoadblockState.Active)
                {
                    game.FiberYield();
                    VerifyIfRoadblockIsBypassed();
                    VerifyIfRoadblockIsHit();
                }
            }, "Roadblock.Monitor");
        }

        private void ReleaseEntitiesToLspdfr()
        {
            DeleteBlip();
            
            foreach (var slot in _slots)
            {
                slot.ReleaseToLspdfr();
            }
        }

        private void VerifyIfRoadblockIsBypassed()
        {
            var currentDistance = _vehicle.DistanceTo(Postion);

            if (currentDistance < _lastKnownDistanceToRoadblock)
            {
                _lastKnownDistanceToRoadblock = currentDistance;
            }
            else if (Math.Abs(currentDistance - _lastKnownDistanceToRoadblock) > BypassTolerance)
            {
                UpdateState(RoadblockState.Bypassed);
                ReleaseEntitiesToLspdfr();
                _logger.Info("Roadblock has been bypassed");
            }
        }

        private void VerifyIfRoadblockIsHit()
        {
            if (_slots.Any(slot => _vehicle.IsTouching(slot.Vehicle)))
            {
                UpdateState(RoadblockState.Hit);
                ReleaseEntitiesToLspdfr();
                _logger.Info("Roadblock has been hit by the suspect");
            }
        }

        private void UpdateState(RoadblockState state)
        {
            State = state;
            RoadblockStateChanged?.Invoke(state);
        }
    }
}