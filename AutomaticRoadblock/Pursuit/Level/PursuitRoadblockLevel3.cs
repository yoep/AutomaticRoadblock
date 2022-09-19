using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    internal class PursuitRoadblockLevel3 : AbstractPursuitRoadblock
    {
        public PursuitRoadblockLevel3(PursuitRoadblockRequest request)
            : base(request.RoadblockData, request.Road, request.TargetVehicle, request.Flags)
        {
        }

        #region Properties

        /// <inheritdoc />
        public override ERoadblockLevel Level => ERoadblockLevel.Level3;

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override IRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
        {
            return new PursuitRoadblockSlotLevel3(lane, MainBarrier, SecondaryBarrier, RetrieveBackupUnitType(), heading, targetVehicle, SlotLightSources(),
                shouldAddLights);
        }

        #endregion
    }
}