using System.Linq;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    public class PursuitRoadblockSlotLevel2 : AbstractPursuitRoadblockSlot
    {
        internal PursuitRoadblockSlotLevel2(Road.Lane lane, BarrierType barrierType, float heading, Vehicle targetVehicle, bool shouldAddLights)
            : base(lane, barrierType, heading, targetVehicle, shouldAddLights)
        {
        }

        public override void Spawn()
        {
            base.Spawn();
            CopInstances
                .ToList()
                .ForEach(x => x.AimAt(TargetVehicle, 45000));
        }

        protected override Model GetVehicleModel()
        {
            return ModelUtils.Vehicles.GetLocalPoliceVehicle(Position, true, false);
        }

        protected override void InitializeCops()
        {
            var isBike = ModelUtils.IsBike(VehicleModel);
            var totalOccupants = isBike ? 1 : 2;
            var pedSpawnPosition = CalculatePositionBehindVehicle();

            for (var i = 0; i < totalOccupants; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.CopPed, GameUtils.GetOnTheGroundPosition(pedSpawnPosition), Heading - 180,
                    (position, heading) => PedFactory.CreateCopWeaponsForModel(new ARPed(RoadblockHelpers.GetPedModelForVehicle(VehicleModel, Position), position, heading))));
                pedSpawnPosition += MathHelper.ConvertHeadingToDirection(Heading + 90) * 1.5f;
            }
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