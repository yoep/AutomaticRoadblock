using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    internal class PursuitRoadblockLevel4 : AbstractPursuitRoadblock
    {
        public PursuitRoadblockLevel4(PursuitRoadblockRequest request)
            : base(request.RoadblockData, request.Road, request.TargetVehicle, request.TargetHeading, request.Flags)
        {
            RoadblockStateChanged += StateChanged;
        }

        #region Properties

        /// <inheritdoc />
        public override ERoadblockLevel Level => ERoadblockLevel.Level4;

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool Spawn()
        {
            var result = base.Spawn();
            SpawnChaseVehicleActions();
            return result;
        }

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override IReadOnlyList<IPursuitRoadblockSlot> CreateRoadblockSlots(IReadOnlyList<Road.Lane> lanesToBlock)
        {
            var slots = base.CreateRoadblockSlots(lanesToBlock);
            var additionalSlots = new List<IPursuitRoadblockSlot>();

            // add additional slots in-between the lanes
            for (var i = 0; i < slots.Count - 1; i++)
            {
                var currentSlot = slots[i];
                var nextSlot = slots[i + 1];
                var distanceToNext = currentSlot.Position.DistanceTo(nextSlot.Position);
                var lane = currentSlot.Lane.MoveTo(MathHelper.ConvertHeadingToDirection(Heading) * 4f +
                                                   MathHelper.ConvertHeadingToDirection(Heading + 90) * (distanceToNext / 2));

                additionalSlots.Add(new PursuitRoadblockSlotLevel4(lane, BarrierModel.None, BarrierModel.None, RetrieveBackupUnitType(), currentSlot.Heading,
                    TargetVehicle,
                    SlotLightSources(), false));
            }

            additionalSlots.AddRange(slots);
            return additionalSlots;
        }

        /// <inheritdoc />
        protected override void InitializeAdditionalVehicles()
        {
            CreateChaseVehicle();
        }

        /// <inheritdoc />
        protected override IPursuitRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
        {
            return new PursuitRoadblockSlotLevel4(lane, MainBarrier, SecondaryBarrier, RetrieveBackupUnitType(), heading, targetVehicle, SlotLightSources(),
                shouldAddLights);
        }

        /// <inheritdoc />
        protected override IList<Ped> RetrieveCopsJoiningThePursuit(bool releaseAll)
        {
            if (releaseAll)
            {
                return base.RetrieveCopsJoiningThePursuit(true);
            }

            if (IsAllowedToJoinPursuit())
            {
                // only the chase vehicle will join the pursuit
                return RetrieveAndReleaseChaseVehicle();
            }

            return Array.Empty<Ped>();
        }

        private void StateChanged(IRoadblock roadblock, ERoadblockState newState)
        {
            if (newState is ERoadblockState.Bypassed or ERoadblockState.Hit)
            {
                var instances = GetValidCopInstances().ToList();
                var vehicleInstance = Instances
                    .Where(x => x.Type == EEntityType.CopVehicle)
                    .Select(x => (ARVehicle)x)
                    .FirstOrDefault();

                RoadblockHelpers.ReleaseInstancesToLspdfr(instances, vehicleInstance);
            }
        }

        #endregion
    }
}