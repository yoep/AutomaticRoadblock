using System.Linq;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    public class PursuitRoadblockSlotLevel5 : AbstractPursuitRoadblockSlot
    {
        internal PursuitRoadblockSlotLevel5(Road.Lane lane, BarrierType barrierType, float heading, Vehicle targetVehicle, bool shouldAddLights)
            : base(lane, barrierType, heading, targetVehicle, shouldAddLights)
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
        protected override Model GetVehicleModel()
        {
            return Random.Next(3) == 0 ? ModelUtils.Vehicles.GetFbiPoliceVehicle() : ModelUtils.Vehicles.GetSwatPoliceVehicle();
        }

        /// <inheritdoc />
        protected override void InitializeCops()
        {
            var pedSpawnPosition = GetPositionBehindVehicle();
            var totalOccupants = Random.Next(3) + 2;

            for (var i = 0; i < totalOccupants; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.CopPed, pedSpawnPosition, 0f,
                    (position, heading) => PedFactory.CreateCopWeapons(new ARPed(GetPedModelForVehicle(), position, heading))));
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
            if (ModelUtils.IsRiot(VehicleModel))
                return base.CalculateVehicleHeading() + 30;

            return base.CalculateVehicleHeading();
        }
    }
}