using System;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Factory;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    internal class PursuitRoadblockLevel5 : AbstractPursuitRoadblock
    {
        public PursuitRoadblockLevel5(Road road, Vehicle vehicle, bool limitSpeed, bool addLights)
            : base(road, BarrierType.PoliceDoNotCross, vehicle, limitSpeed, addLights)
        {
        }

        #region Properties

        /// <inheritdoc />
        public override RoadblockLevel Level => RoadblockLevel.Level5;

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override void InitializeScenery()
        {
            var position = Postion + MathHelper.ConvertHeadingToDirection(Road.Node.Heading - 180) * 3f;

            Instances.Add(new InstanceSlot(EntityType.Scenery, position, 0f,
                (conePosition, _) => BarrierFactory.Create(BarrierType.BigCone, conePosition)));
        }

        /// <inheritdoc />
        protected override void InitializeLights()
        {
            var roadRightSidePosition = Road.RightSide + MathHelper.ConvertHeadingToDirection(Heading) * 5f;
            var roadLeftSidePosition = Road.LeftSide + MathHelper.ConvertHeadingToDirection(Heading) * 5f;

            Instances.Add(new InstanceSlot(EntityType.Scenery, roadRightSidePosition, Math.Abs(Heading - 225) % 360,
                (position, heading) => new ARScenery(PropUtils.CreateGeneratorWithLights(position, heading))));
            Instances.Add(new InstanceSlot(EntityType.Scenery, roadLeftSidePosition, Math.Abs(Heading + 225) % 360,
                (position, heading) => new ARScenery(PropUtils.CreateGeneratorWithLights(position, heading))));
        }

        /// <inheritdoc />
        protected override IRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
        {
            return new PursuitRoadblockSlotLevel5(lane, MainBarrierType, heading, targetVehicle, shouldAddLights);
        }

        #endregion
    }
}