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
    public class PursuitRoadblockSlotLevel3 : AbstractPursuitRoadblockSlot
    {
        internal PursuitRoadblockSlotLevel3(Road.Lane lane, BarrierType barrierType, float heading, Vehicle targetVehicle, bool shouldAddLights)
            : base(lane, barrierType, DetermineVehicleType(), heading, targetVehicle, shouldAddLights)
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
            var pedSpawnPosition = CalculatePositionBehindVehicle();
            var totalOccupants = isBike ? 1 : Random.Next(1, 3);

            for (var i = 0; i < totalOccupants; i++)
            {
                Instances.Add(new InstanceSlot(EEntityType.CopPed, GameUtils.GetOnTheGroundPosition(pedSpawnPosition), 0f,
                    (position, heading) =>
                        PedFactory.CreateCopWeaponsForModel(PedFactory.CreateCopForVehicle(VehicleModel, position, heading))));
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

        private static VehicleType DetermineVehicleType()
        {
            return Random.Next(5) switch
            {
                0 => VehicleType.Transporter,
                1 => VehicleType.State,
                _ => VehicleType.Local
            };
        }
    }
}