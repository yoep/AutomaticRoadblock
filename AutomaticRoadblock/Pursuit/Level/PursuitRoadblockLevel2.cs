using System.Collections.Generic;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    internal class PursuitRoadblockLevel2 : AbstractPursuitRoadblock
    {
        public PursuitRoadblockLevel2(Road street, BarrierModel mainBarrier, BarrierModel secondaryBarrier, Vehicle targetVehicle,
            List<LightModel> lightSources, ERoadblockFlags flags)
            : base(street, mainBarrier, secondaryBarrier, targetVehicle, lightSources, flags)
        {
        }

        #region Properties

        /// <inheritdoc />
        public override ERoadblockLevel Level => ERoadblockLevel.Level2;

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override IRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
        {
            return new PursuitRoadblockSlotLevel2(lane, MainBarrier, SecondaryBarrier, heading, targetVehicle, SlotLightSources(), shouldAddLights);
        }

        #endregion
    }
}