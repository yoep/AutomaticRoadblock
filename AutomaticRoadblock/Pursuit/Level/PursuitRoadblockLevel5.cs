using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    internal class PursuitRoadblockLevel5 : AbstractPursuitRoadblock
    {
        public PursuitRoadblockLevel5(Road street, BarrierModel mainBarrier, BarrierModel secondaryBarrier, BarrierModel chaseVehicleBarrier,
            Vehicle targetVehicle, List<LightModel> lightSources, ERoadblockFlags flags)
            : base(street, mainBarrier, secondaryBarrier, chaseVehicleBarrier, targetVehicle, lightSources, flags)
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
            CreateChaseVehicle(ModelUtils.Vehicles.GetFbiPoliceVehicle());
        }

        /// <inheritdoc />
        protected override IRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
        {
            return new PursuitRoadblockSlotLevel5(lane, MainBarrier, SecondaryBarrier, heading, targetVehicle, SlotLightSources(), shouldAddLights);
        }

        /// <inheritdoc />
        protected override IEnumerable<Ped> RetrieveCopsJoiningThePursuit()
        {
            // only the chase vehicle will join the pursuit
            return Instances
                .Where(x => x.Type == EEntityType.CopPed)
                .Select(x => x.Instance)
                .Select(x => (ARPed)x)
                .Select(x => x.GameInstance)
                .ToList();
        }

        #endregion
    }
}