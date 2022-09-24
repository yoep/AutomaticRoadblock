using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    public class PursuitRoadblockSlotLevel5 : AbstractPursuitRoadblockSlot
    {
        internal PursuitRoadblockSlotLevel5(Road.Lane lane, BarrierModel mainBarrier, BarrierModel secondaryBarrier, EBackupUnit backupUnit, float heading,
            Vehicle targetVehicle,
            List<LightModel> lightSources, bool shouldAddLights)
            : base(lane, mainBarrier, secondaryBarrier, backupUnit, heading, targetVehicle, lightSources, shouldAddLights)
        {
        }

        /// <inheritdoc />
        public override void Spawn()
        {
            base.Spawn();
            CopInstances
                .ToList()
                .ForEach(x => x.FireAt(TargetVehicle, 60000));
        }

        /// <inheritdoc />
        protected override void InitializeScenery()
        {
            // no-op
        }

        /// <inheritdoc />
        protected override float CalculateVehicleHeading()
        {
            if (VehicleModel.Name.Equals("Riot"))
                return base.CalculateVehicleHeading() + 30;

            return base.CalculateVehicleHeading();
        }
    }
}