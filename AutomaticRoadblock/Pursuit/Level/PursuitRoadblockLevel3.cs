using System.Collections.Generic;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    internal class PursuitRoadblockLevel3 : AbstractPursuitRoadblock
    {
        public PursuitRoadblockLevel3(Road street, BarrierModel mainBarrier, BarrierModel secondaryBarrier, Vehicle targetVehicle,
            List<LightModel> lightSources, ERoadblockFlags flags)
            : base(street, mainBarrier, secondaryBarrier, targetVehicle, lightSources, flags)
        {
        }

        #region Properties

        /// <inheritdoc />
        public override ERoadblockLevel Level => ERoadblockLevel.Level3;

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override void InitializeLights()
        {
            // Instances.AddRange(LightSourceRoadblockFactory.CreateGeneratorLights(this));
            // Instances.AddRange(LightSourceRoadblockFactory.CreateAlternatingGroundLights(this, 4));
        }

        /// <inheritdoc />
        protected override IRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
        {
            return new PursuitRoadblockSlotLevel3(lane, MainBarrier, SecondaryBarrier, heading, targetVehicle, shouldAddLights);
        }

        #endregion
    }
}