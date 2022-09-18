using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;
using AutomaticRoadblocks.Utils;
using Rage;
using VehicleType = AutomaticRoadblocks.Vehicles.VehicleType;

namespace AutomaticRoadblocks.Pursuit.Level
{
    public class PursuitRoadblockSlotLevel2 : AbstractPursuitRoadblockSlot
    {
        internal PursuitRoadblockSlotLevel2(Road.Lane lane, BarrierModel mainBarrier, BarrierModel secondaryBarrier, float heading, Vehicle targetVehicle,
            bool shouldAddLights)
            : base(lane, mainBarrier, secondaryBarrier, VehicleType.LocalUnit, heading, targetVehicle, shouldAddLights)
        {
        }

        public override void Spawn()
        {
            base.Spawn();
            CopInstances
                .ToList()
                .ForEach(x => x.AimAt(TargetVehicle, 45000));
        }

        protected override void InitializeCops()
        {
            var isBike = ModelUtils.Vehicles.IsBike(VehicleModel);
            var totalOccupants = isBike ? 1 : Random.Next(1, 3);
            var pedSpawnPosition = CalculatePositionBehindVehicle();

            for (var i = 0; i < totalOccupants; i++)
            {
                Instances.Add(new InstanceSlot(EEntityType.CopPed, GameUtils.GetOnTheGroundPosition(pedSpawnPosition), Heading - 180,
                    (position, heading) => PedFactory.CreateCopWeaponsForModel(PedFactory.CreateCopForVehicle(VehicleModel, position, heading))));
                pedSpawnPosition += MathHelper.ConvertHeadingToDirection(Heading + 90) * 1.5f;
            }
        }

        protected override void InitializeScenery()
        {
            // no-op
        }

        protected override void InitializeLights()
        {
            // Instances.AddRange(LightSourceSlotFactory.Create(LightSourceType.Flares, this));
        }
    }
}