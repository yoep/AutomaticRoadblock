using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Utils.Type;
using Rage;
using VehicleType = AutomaticRoadblocks.Vehicles.VehicleType;

namespace AutomaticRoadblocks.Pursuit.Level
{
    public class PursuitRoadblockSlotLevel1 : AbstractPursuitRoadblockSlot
    {
        internal PursuitRoadblockSlotLevel1(Road.Lane lane, BarrierType barrierType, float heading, Vehicle targetVehicle, bool shouldAddLights)
            : base(lane, barrierType, VehicleType.Local, heading, targetVehicle, shouldAddLights)
        {
        }

        public override void Spawn()
        {
            base.Spawn();
            CopInstances.ToList()
                .ForEach(x => x.WarpIntoVehicle(Vehicle, EVehicleSeat.Driver));
        }

        protected override void InitializeCops()
        {
            Instances.Add(new InstanceSlot(EEntityType.CopPed, GameUtils.GetOnTheGroundPosition(Position), 0f, (position, heading) =>
                PedFactory.CreateCopWeaponsForModel(PedFactory.CreateCopForVehicle(VehicleModel, position, heading))));
        }

        protected override void InitializeScenery()
        {
            // no-op
        }

        protected override void InitializeLights()
        {
            Instances.AddRange(LightSourceSlotFactory.Create(LightSourceType.Flares, this));
        }
    }
}