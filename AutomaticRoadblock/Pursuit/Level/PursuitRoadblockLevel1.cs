using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    internal class PursuitRoadblockLevel1 : AbstractPursuitRoadblock
    {
        public PursuitRoadblockLevel1(PursuitRoadblockRequest request)
            : base(request.RoadblockData, request.Road, request.TargetVehicle, request.TargetHeading, request.Flags)
        {
        }

        #region Properties

        /// <inheritdoc />
        public override ERoadblockLevel Level => ERoadblockLevel.Level1;

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override IReadOnlyList<Road.Lane> LanesToBlock()
        {
            var lanesToBlock = base.LanesToBlock();

            // only block the lanes in the same direction as the pursuit
            return lanesToBlock
                .Where(x => Math.Abs(x.Heading - TargetHeading) < LaneHeadingTolerance)
                .ToList();
        }

        /// <inheritdoc />
        protected override IRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
        {
            return new PursuitRoadblockSlotLevel1(lane, MainBarrier, SecondaryBarrier, RetrieveBackupUnitType(), heading, targetVehicle, SlotLightSources(),
                shouldAddLights);
        }

        #endregion
    }
}