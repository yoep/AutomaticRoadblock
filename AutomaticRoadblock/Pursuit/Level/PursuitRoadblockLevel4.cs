using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Factory;
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
            var position = Position + MathHelper.ConvertHeadingToDirection(Road.Node.Heading - 180) * 3f;

            Instances.Add(new InstanceSlot(EntityType.Scenery, position, 0f,
                (conePosition, _) => BarrierFactory.Create(BarrierType.BigCone, conePosition)));

            // add a chase vehicle on the right side of the road
            CreateChaseVehicle();
        }

        /// <inheritdoc />
        protected override void InitializeLights()
        {
            Instances.AddRange(LightSourceRoadblockFactory.CreateGeneratorLights(this));
            Instances.AddRange(LightSourceRoadblockFactory.CreateRedBlueGroundLights(this, 3));
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

        private void CreateChaseVehicle()
        {
            var roadPosition = Road.RightSide + ChaseVehiclePositionDirection();
            var vehicleModel = ModelUtils.Vehicles.GetLocalPoliceVehicle(roadPosition, false, false);

            Instances.AddRange(new[]
            {
                new InstanceSlot(EntityType.CopVehicle, roadPosition, TargetHeading + 25,
                    (position, heading) => new ARVehicle(vehicleModel, GameUtils.GetOnTheGroundPosition(position), heading)),
                new InstanceSlot(EntityType.CopPed, roadPosition, TargetHeading,
                    (position, heading) =>
                        PedFactory.CreateCopWeaponsForModel(new ARPed(ModelUtils.Peds.GetLocalCop(roadPosition), GameUtils.GetOnTheGroundPosition(position),
                            heading)))
            });

            // create buffer barrels behind the vehicle
            CreateChaseVehicleBufferBarrels(roadPosition);
        }

        private void CreateChaseVehicleBufferBarrels(Vector3 chasePosition)
        {
            var rowPosition = chasePosition + MathHelper.ConvertHeadingToDirection(TargetHeading - 180) * 4f;
            var nextPositionDistance = BarrierType.BarrelTrafficCatcher.Width + BarrierType.BarrelTrafficCatcher.Spacing;
            var nextPositionDirection = MathHelper.ConvertHeadingToDirection(TargetHeading - 90);
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(TargetHeading + 90) * (nextPositionDistance * 2f);

            for (var i = 0; i < 5; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.Scenery, startPosition, TargetHeading,
                    (position, heading) => BarrierFactory.Create(BarrierType.BarrelTrafficCatcher, position, heading)));
                startPosition += nextPositionDirection * nextPositionDistance;
            }
        }

        private Vector3 ChaseVehiclePositionDirection()
        {
            return MathHelper.ConvertHeadingToDirection(TargetHeading) * 15f +
                   MathHelper.ConvertHeadingToDirection(TargetHeading - 90) * 1.5f;
        }

        #endregion
    }
}