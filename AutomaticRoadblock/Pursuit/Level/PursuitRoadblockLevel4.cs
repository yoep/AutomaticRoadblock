using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using LSPD_First_Response.Engine.Scripting.Entities;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    internal class PursuitRoadblockLevel4 : AbstractPursuitRoadblock
    {
        public PursuitRoadblockLevel4(Road road, Vehicle vehicle, bool limitSpeed, bool addLights)
            : base(road, BarrierType.PoliceDoNotCross, vehicle, limitSpeed, addLights)
        {
            RoadblockStateChanged += StateChanged;
        }

        #region Properties

        /// <inheritdoc />
        public override RoadblockLevel Level => RoadblockLevel.Level4;

        #endregion

        #region Methods

        public override void Spawn()
        {
            base.Spawn();
            var vehicle = Instances
                .Where(x => x.Type == EntityType.CopVehicle)
                .Select(x => x.Instance)
                .Select(x => (ARVehicle)x)
                .Select(x => x.GameInstance)
                .First();

            Instances
                .Where(x => x.Type == EntityType.CopPed)
                .Select(x => x.Instance)
                .Select(x => (ARPed)x)
                .Select(x => x.GameInstance)
                .ToList()
                .ForEach(x => x.WarpIntoVehicle(vehicle, (int)VehicleSeat.Any));
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

                additionalSlots.Add(new PursuitRoadblockSlotLevel4(lane, BarrierType.None, currentSlot.Heading, Vehicle, false));
            }

            additionalSlots.AddRange(slots);
            return additionalSlots;
        }

        /// <inheritdoc />
        protected override void InitializeScenery()
        {
            var position = Position + MathHelper.ConvertHeadingToDirection(Road.Node.Heading - 180) * 2f;

            Instances.Add(new InstanceSlot(EntityType.Scenery, position, 0f,
                (conePosition, _) => BarrierFactory.Create(BarrierType.BigCone, conePosition)));
        }

        /// <inheritdoc />
        protected override void InitializeLights()
        {
            Instances.AddRange(LightSourceRoadblockFactory.CreateGeneratorLights(this));
            Instances.AddRange(LightSourceRoadblockFactory.CreateRedBlueGroundLights(this, 3));
        }

        /// <inheritdoc />
        protected override void InitializeAdditionalVehicles()
        {
            CreateChaseVehicle(ModelUtils.Vehicles.GetLocalPoliceVehicle(Position));
        }

        /// <inheritdoc />
        protected override IRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
        {
            return new PursuitRoadblockSlotLevel4(lane, MainBarrierType, heading, targetVehicle, shouldAddLights);
        }

        /// <inheritdoc />
        protected override IEnumerable<Ped> RetrieveCopsJoiningThePursuit()
        {
            // only the chase vehicle will join the pursuit
            return Instances
                .Where(x => x.Type == EntityType.CopPed)
                .Select(x => x.Instance)
                .Select(x => (ARPed)x)
                .Select(x => x.GameInstance)
                .ToList();
        }

        private void StateChanged(IRoadblock roadblock, RoadblockState newState)
        {
            if (newState is RoadblockState.Bypassed or RoadblockState.Hit)
                RoadblockHelpers.ReleaseInstancesToLspdfr(Instances, Vehicle);
        }

        #endregion
    }
}