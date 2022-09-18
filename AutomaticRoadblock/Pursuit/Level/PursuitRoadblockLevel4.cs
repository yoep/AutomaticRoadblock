using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    internal class PursuitRoadblockLevel4 : AbstractPursuitRoadblock
    {
        public PursuitRoadblockLevel4(Road street, BarrierModel mainBarrier, BarrierModel secondaryBarrier, BarrierModel chaseVehicleBarrier,
            Vehicle targetVehicle, List<LightModel> lightSources, ERoadblockFlags flags)
            : base(street, mainBarrier, secondaryBarrier, chaseVehicleBarrier, targetVehicle, lightSources, flags)
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

                additionalSlots.Add(new PursuitRoadblockSlotLevel4(lane, BarrierModel.None, BarrierModel.None, currentSlot.Heading, TargetVehicle,
                    SlotLightSources(), false));
            }

            additionalSlots.AddRange(slots);
            return additionalSlots;
        }

        /// <inheritdoc />
        protected override void InitializeAdditionalVehicles()
        {
            CreateChaseVehicle(ModelUtils.Vehicles.GetLocalPoliceVehicle(Position));
        }

        /// <inheritdoc />
        protected override IRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
        {
            return new PursuitRoadblockSlotLevel4(lane, MainBarrier, SecondaryBarrier, heading, targetVehicle, SlotLightSources(), shouldAddLights);
        }

        /// <inheritdoc />
        protected override IEnumerable<Ped> RetrieveCopsJoiningThePursuit()
        {
            // only the chase vehicle will join the pursuit
            return Instances
                .Where(x => x.Type == EEntityType.CopPed)
                .Select(x => x.Instance)
                .Select(x => (ARPed)x)
                .Select(x => x.GameInstance)
                .ToList();
        }

        private void StateChanged(IRoadblock roadblock, ERoadblockState newState)
        {
            if (newState is ERoadblockState.Bypassed or ERoadblockState.Hit)
                RoadblockHelpers.ReleaseInstancesToLspdfr(Instances, TargetVehicle);
        }

        #endregion
    }
}