using System.Linq;
using AutomaticRoadblocks.Instance;
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
                .ForEach(x =>
                {
                    x.EquipPrimaryWeapon();
                    Logger.Trace("Taking cover from target vehicle driver");
                    x.Cover();
                    x.GameInstance.Tasks.TakeCoverAt(Vehicle.Position + MathHelper.ConvertHeadingToDirection(Heading) * 2f, TargetVehicle.Position, -1, false);
                });
        }

        protected override Model GetVehicleModel()
        {
            return ModelUtils.Vehicles.GetLocalPoliceVehicle(Position, true, false);
        }

        protected override void InitializeCopPeds()
        {
            var isBike = ModelUtils.IsBike(VehicleModel);
            var totalOccupants = isBike ? 1 : 2;
            var pedSpawnPosition = GetPositionBehindVehicle();

            for (var i = 0; i < totalOccupants; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.CopPed, pedSpawnPosition, Heading - 180,
                    (position, heading) => PedFactory.CreateCopWeapons(new ARPed(GetPedModelForVehicle(), position, heading))));
                pedSpawnPosition += MathHelper.ConvertHeadingToDirection(Heading + 90) * 1.5f;
            }
        }

        protected override void InitializeScenery()
        {
            // no-op
        }

        protected override void InitializeLights()
        {
            var rowPosition = Position + MathHelper.ConvertHeadingToDirection(Heading - 180) * 2f;
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(Heading + 90) * 3f;
            var direction = MathHelper.ConvertHeadingToDirection(Heading - 90);
            var totalFlares = (int)Lane.Width;

            for (var i = 0; i < totalFlares; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.Scenery, startPosition, Heading,
                    (position, heading) => new ARScenery(PropUtils.CreateHorizontalFlare(position, heading + Random.Next(91)))));
                startPosition += direction * 1f;
            }
        }
    }
}