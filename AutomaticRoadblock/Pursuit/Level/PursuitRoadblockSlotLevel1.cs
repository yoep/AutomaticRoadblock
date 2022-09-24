using System.Collections.Generic;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    public class PursuitRoadblockSlotLevel1 : AbstractPursuitRoadblockSlot
    {
        internal PursuitRoadblockSlotLevel1(Road.Lane lane, BarrierModel mainBarrier, BarrierModel secondaryBarrier, EBackupUnit backupUnit, float heading,
            Vehicle targetVehicle,
            List<LightModel> lightSources, bool shouldAddLights)
            : base(lane, mainBarrier, secondaryBarrier, backupUnit, heading, targetVehicle, lightSources, shouldAddLights)
        {
        }

        public override void Spawn()
        {
            base.Spawn();
            WarpInVehicle();
        }

        protected override void InitializeScenery()
        {
            // no-op
        }
    }
}