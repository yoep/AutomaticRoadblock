using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class ManualRoadblockSlot : AbstractRoadblockSlot
    {
        public ManualRoadblockSlot(Road.Lane lane, BarrierType barrierType, float heading, bool shouldAddLights) 
            : base(lane, barrierType, heading, shouldAddLights)
        {
        }

        protected override void InitializeCopPeds()
        {
            // no-op
        }

        protected override void InitializeScenery()
        {
            // no-op
        }

        protected override void InitializeLights()
        {
            // no-op
        }

        protected override Model GetVehicleModel()
        {
            return ModelUtils.Vehicles.GetLocalPoliceVehicle(Position);
        }
    }
}