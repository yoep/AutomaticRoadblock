using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
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
            var pedSpawnPosition = CalculatePositionBehindVehicle();

            for (var i = 0; i < 2; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.CopPed, GameUtils.GetOnTheGroundPosition(pedSpawnPosition), 0f,
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
                _ => VehicleType.Locale
            };
        }
    }
}