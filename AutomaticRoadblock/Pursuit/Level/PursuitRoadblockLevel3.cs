using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    internal class PursuitRoadblockLevel3 : AbstractPursuitRoadblock
    {
        public PursuitRoadblockLevel3(Road road, Vehicle vehicle, bool limitSpeed, bool addLights)
            : base(road, BarrierType.PoliceDoNotCross, vehicle, limitSpeed, addLights)
        {
        }

        #region Properties

        /// <inheritdoc />
        public override RoadblockLevel Level => RoadblockLevel.Level3;

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override void InitializeScenery()
        {
            var position = Position + MathHelper.ConvertHeadingToDirection(Road.Node.Heading - 180) * 2f;

            Instances.Add(new InstanceSlot(EEntityType.Scenery, position, 0f,
                (conePosition, _) => BarrierFactory.Create(BarrierType.BigCone, conePosition)));
        }

        /// <inheritdoc />
        protected override void InitializeLights()
        {
            Instances.AddRange(LightSourceRoadblockFactory.CreateGeneratorLights(this));
            Instances.AddRange(LightSourceRoadblockFactory.CreateAlternatingGroundLights(this, 4));
        }

        /// <inheritdoc />
        protected override IRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
        {
            return new PursuitRoadblockSlotLevel3(lane, MainBarrierType, heading, targetVehicle, shouldAddLights);
        }

        #endregion
    }
}