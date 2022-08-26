using System.Linq;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    public class PursuitRoadblockSlotLevel4 : AbstractPursuitRoadblockSlot
    {
        internal PursuitRoadblockSlotLevel4(Road.Lane lane, BarrierType barrierType, float heading, Vehicle targetVehicle, bool shouldAddLights)
            : base(lane, barrierType, heading, targetVehicle, shouldAddLights)
        {
        }

        public override void Spawn()
        {
            base.Spawn();
            CopInstances
                .ToList()
                .ForEach(x => x.FireAt(TargetVehicle, 60000));
        }

        protected override Model GetVehicleModel()
        {
            return Random.Next(3) == 0 ? ModelUtils.Vehicles.GetStatePoliceVehicle(false) : ModelUtils.Vehicles.GetFbiPoliceVehicle();
        }

        protected override void InitializeCops()
        {
            var pedSpawnPosition = GetPositionBehindVehicle();

            for (var i = 0; i < 2; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.CopPed, GameUtils.GetOnTheGroundPosition(pedSpawnPosition), 0f,
                    (position, heading) => PedFactory.CreateCopWeaponsForModel(new ARPed(GetPedModelForVehicle(), position, heading))));
                pedSpawnPosition += MathHelper.ConvertHeadingToDirection(Heading + 90) * 1.5f;
            }
        }

        protected override void InitializeScenery()
        {
            // no-op
        }

        protected override void InitializeLights()
        {
        }
    }
}