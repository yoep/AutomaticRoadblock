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
        protected override IReadOnlyList<IRoadblockSlot> CreateRoadblockSlots(IReadOnlyList<Road.Lane> lanesToBlock)
        {
            var slots = base.CreateRoadblockSlots(lanesToBlock);
            var additionalSlots = new List<IRoadblockSlot>();

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
            CreateChaseVehicle(RetrieveVehicleModel());
        }

        /// <inheritdoc />
        protected override IRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
        {
            return new PursuitRoadblockSlotLevel4(lane, MainBarrier, SecondaryBarrier, RetrieveBackupUnitType(), heading, targetVehicle, SlotLightSources(),
                shouldAddLights);
        }

        /// <param name="releaseAll"></param>
        /// <inheritdoc />
        protected override IEnumerable<Ped> RetrieveCopsJoiningThePursuit(bool releaseAll)
        {
            // only the chase vehicle will join the pursuit
            return GetValidCopInstances()
                .Select(x => x.GameInstance)
                .ToList();
        }

        private void StateChanged(IRoadblock roadblock, ERoadblockState newState)
        {
            if (newState is ERoadblockState.Bypassed or ERoadblockState.Hit)
            {
                var instances = GetValidCopInstances().ToList();
                var vehicleInstance = Instances
                    .Where(x => x.Type == EEntityType.CopVehicle)
                    .Select(x => x.Instance)
                    .Select(x => (ARVehicle)x)
                    .FirstOrDefault();

                RoadblockHelpers.ReleaseInstancesToLspdfr(instances, vehicleInstance);
            }
        }

        private IEnumerable<ARPed> GetValidCopInstances()
        {
            return Instances
                .Where(x => x.Type == EEntityType.CopPed)
                .Select(x => x.Instance)
                .Where(x => x is { IsInvalid: false })
                .Select(x => (ARPed)x);
        }

        #endregion
    }
}