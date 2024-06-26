using System;
using System.Collections.Generic;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    internal class PursuitRoadblockLevel5 : AbstractPursuitRoadblock
    {
        public PursuitRoadblockLevel5(PursuitRoadblockRequest request)
            : base(request.RoadblockData, request.Road, request.TargetVehicle, request.TargetHeading, request.Flags)
        {
        }

        #region Properties

        /// <inheritdoc />
        public override ERoadblockLevel Level => ERoadblockLevel.Level5;

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool Spawn()
        {
            var result = base.Spawn();
            SpawnChaseVehicleActions();
            return result;
        }

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override void InitializeAdditionalVehicles()
        {
            CreateChaseVehicle();
        }

        /// <inheritdoc />
        protected override IPursuitRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
        {
            return new PursuitRoadblockSlotLevel5(lane, MainBarrier, SecondaryBarrier, RetrieveBackupUnitType(), heading, targetVehicle, SlotLightSources(),
                shouldAddLights);
        }

        /// <param name="releaseAll"></param>
        /// <inheritdoc />
        protected override IList<Ped> RetrieveCopsJoiningThePursuit(bool releaseAll)
        {
            if (releaseAll)
            {
                return base.RetrieveCopsJoiningThePursuit(true);
            }

            if (IsAllowedToJoinPursuit())
            {
                // only the chase vehicle will join the pursuit
                return RetrieveAndReleaseChaseVehicle();
            }

            return Array.Empty<Ped>();
        }

        #endregion
    }
}