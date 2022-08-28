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
    public class PursuitRoadblockSlotLevel5 : AbstractPursuitRoadblockSlot
    {
        internal PursuitRoadblockSlotLevel5(Road.Lane lane, BarrierType barrierType, float heading, Vehicle targetVehicle, bool shouldAddLights)
            : base(lane, barrierType, DetermineVehicleType(), heading, targetVehicle, shouldAddLights)
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
        protected override void InitializeCops()
        {
            var pedSpawnPosition = CalculatePositionBehindVehicle();
            var totalOccupants = Random.Next(3) + 2;

            for (var i = 0; i < totalOccupants; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.CopPed, GameUtils.GetOnTheGroundPosition(pedSpawnPosition), 0f,
                    (position, heading) => PedFactory.CreateCopWeaponsForModel(PedFactory.CreateCopForVehicle(VehicleModel, position, heading))));
                pedSpawnPosition += MathHelper.ConvertHeadingToDirection(Heading + 90) * 1.5f;
            }
        }

        /// <inheritdoc />
        protected override void InitializeScenery()
        {
            // no-op
        }

        /// <inheritdoc />
        protected override void InitializeLights()
        {
        }

        /// <inheritdoc />
        protected override float CalculateVehicleHeading()
        {
            if (ModelUtils.Vehicles.IsRiot(VehicleModel))
                return base.CalculateVehicleHeading() + 30;

            return base.CalculateVehicleHeading();
        }

        private static VehicleType DetermineVehicleType()
        {
            return Random.Next(4) switch
            {
                0 => VehicleType.Swat,
                _ => VehicleType.Fbi,
            };
        }
    }
}