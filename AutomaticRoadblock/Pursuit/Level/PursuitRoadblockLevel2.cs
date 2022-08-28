using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    internal class PursuitRoadblockLevel2 : AbstractPursuitRoadblock
    {
        public PursuitRoadblockLevel2(Road road, Vehicle vehicle, bool limitSpeed, bool addLights)
            : base(road, BarrierType.BigCone, vehicle, limitSpeed, addLights)
        {
        }

        #region Properties

        /// <inheritdoc />
        public override RoadblockLevel Level => RoadblockLevel.Level2;

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override void InitializeScenery()
        {
            var position = Position + MathHelper.ConvertHeadingToDirection(Road.Node.Heading - 180) * 2f;

            Instances.Add(new InstanceSlot(EntityType.Scenery, GameUtils.GetOnTheGroundPosition(position), 0f,
                (conePosition, _) => new ARScenery(PropUtils.CreateBigConeWithStripes(conePosition))));
        }

        /// <inheritdoc />
        protected override void InitializeLights()
        {
            // no-op
        }

        /// <inheritdoc />
        protected override IRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
        {
            return new PursuitRoadblockSlotLevel2(lane, MainBarrierType, heading, targetVehicle, shouldAddLights);
        }

        #endregion
    }
}