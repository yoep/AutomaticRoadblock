using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    internal class PursuitRoadblockLevel2 : AbstractPursuitRoadblock
    {
        public PursuitRoadblockLevel2( Road street, Vehicle targetVehicle, ERoadblockFlags flags)
            : base(street, BarrierType.BigCone, targetVehicle, flags)
        {
        }

        #region Properties

        /// <inheritdoc />
        public override ERoadblockLevel Level => ERoadblockLevel.Level2;

        #endregion

        #region Functions

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