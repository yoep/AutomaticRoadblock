using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    public class RoadblockLevel2 : AbstractRoadblock
    {
        public RoadblockLevel2(Road road, Vehicle vehicle, bool limitSpeed, bool addLights) 
            : base(road, vehicle, limitSpeed, addLights)
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
            var position = Postion + MathHelper.ConvertHeadingToDirection(Road.Node.Heading - 180) * 3f;

            Instances.Add(new InstanceSlot(EntityType.Scenery, position, 0f,
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
            return new RoadblockSlotLevel2(lane, heading, targetVehicle, shouldAddLights);
        }

        #endregion
    }
}