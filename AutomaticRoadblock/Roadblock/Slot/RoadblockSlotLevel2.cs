using System.Linq;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public class RoadblockSlotLevel2 : AbstractRoadblockSlot
    {
        internal RoadblockSlotLevel2(Vector3 position, float heading, Vehicle targetVehicle) : base(position, heading, targetVehicle)
        {
            Init();
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

        private void Init()
        {
            InitializeVehicleSlot();
            InitializePedSlots();
            InitializeBarriers();
        }

        private void InitializePedSlots()
        {
            var isBike = ModelUtils.IsBike(VehicleModel);
            var totalOccupants = isBike ? 1 : 2;
            var pedSpawnPosition = GetPositionBehindVehicle();

            for (var i = 0; i < totalOccupants; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.CopPed, pedSpawnPosition, Heading - 180,
                    (position, heading) => AssignCopWeapons(new ARPed(ModelUtils.GetLocalCop(Position), position, heading))));
                pedSpawnPosition += MathHelper.ConvertHeadingToDirection(Heading + 90) * 1.5f;
            }
        }

        private void InitializeBarriers()
        {
            var rowPosition = Position + MathHelper.ConvertHeadingToDirection(Heading - 180) * 3f;
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(Heading + 90) * 2f;

            for (var i = 0; i < 2; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.Barrier, startPosition, Heading,
                    (position, heading) => new ARScenery(PropUtils.CreatePoliceDoNotCrossBarrier(position, heading))));
                startPosition += MathHelper.ConvertHeadingToDirection(Heading - 90) * 3f;
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