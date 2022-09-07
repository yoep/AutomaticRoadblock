using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.SpikeStrip.Dispatcher;
using AutomaticRoadblocks.Street;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    internal class PursuitRoadblockLevel2 : AbstractPursuitRoadblock
    {
        public PursuitRoadblockLevel2(ISpikeStripDispatcher spikeStripDispatcher, Road street, Vehicle targetVehicle, bool limitSpeed, bool addLights,
            bool spikeStripEnabled)
            : base(spikeStripDispatcher, street, BarrierType.BigCone, targetVehicle, limitSpeed, addLights, spikeStripEnabled)
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

            Instances.Add(new InstanceSlot(EEntityType.Scenery, position, 0f,
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