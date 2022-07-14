using System.Linq;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public class RoadblockSlotLevel2 : AbstractRoadblockSlot
    {
        internal RoadblockSlotLevel2(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
            : base(lane, heading, targetVehicle, shouldAddLights)
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
            return ModelUtils.GetLocalPoliceVehicle(Position, true, false);
        }

        protected override void InitializeCopPeds()
        {
            var isBike = ModelUtils.IsBike(VehicleModel);
            var totalOccupants = isBike ? 1 : 2;
            var pedSpawnPosition = GetPositionBehindVehicle();

            for (var i = 0; i < totalOccupants; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.CopPed, pedSpawnPosition, Heading - 180,
                    (position, heading) => AssignCopWeapons(new ARPed(GetPedModelForVehicle(), position, heading))));
                pedSpawnPosition += MathHelper.ConvertHeadingToDirection(Heading + 90) * 1.5f;
            }
        }

        protected override void InitializeScenery()
        {
            var rowPosition = Position + MathHelper.ConvertHeadingToDirection(Heading - 180) * 3f;
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(Heading + 90) * 2f;

            for (var i = 0; i < 2; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.Scenery, startPosition, Heading,
                    (position, heading) => new ARScenery(PropUtils.CreatePoliceDoNotCrossBarrier(position, heading))));
                startPosition += MathHelper.ConvertHeadingToDirection(Heading - 90) * 3f;
            }
        }

        protected override void InitializeLights()
        {
            var rowPosition = Position + MathHelper.ConvertHeadingToDirection(Heading - 180) * 2f;
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(Heading + 90) * 3f;
            var direction = MathHelper.ConvertHeadingToDirection(Heading - 90);
            var totalFlares = (int) Lane.Width;

            for (var i = 0; i < totalFlares; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.Scenery, startPosition, Heading,
                    (position, heading) => new ARScenery(PropUtils.CreateFlare(position, heading + Random.Next(91)))));
                startPosition += direction * 1f;
            }
        }

        private static ARPed AssignCopWeapons(ARPed ped)
        {
            ped.GivePrimaryWeapon(ModelUtils.Weapons.Pistol);
            ped.GiveWeapon(ModelUtils.Weapons.Nightstick);
            ped.GiveWeapon(ModelUtils.Weapons.StunGun);
            ped.GiveWeapon(ModelUtils.Weapons.Shotgun);
            return ped;
        }
    }
}