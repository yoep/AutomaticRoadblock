using System;
using System.Linq;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public class RoadblockSlotLevel5 : AbstractRoadblockSlot
    {
        internal RoadblockSlotLevel5(Vector3 position, float heading, Vehicle targetVehicle) : base(position, heading, targetVehicle)
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
                    x.GameInstance.Tasks.TakeCoverFrom(TargetVehicle.Driver, -1, true);
                });
        }

        protected override Model GetVehicleModel()
        {
            return ModelUtils.GetSwatPoliceVehicle();
        }

        private void Init()
        {
            InitializeVehicleSlot();
            InitializePedSlots();
            InitializeBarriers();
        }

        private void InitializePedSlots()
        {
            var pedSpawnPosition = GetPositionBehindVehicle();
            var totalOccupants = new Random().Next(3) + 2;

            for (var i = 0; i < totalOccupants; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.CopPed, pedSpawnPosition, 0f,
                    (position, heading) => AssignCopWeapons(new ARPed(ModelUtils.GetPoliceSwatCop(), position, heading))));
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
            ped.GivePrimaryWeapon(ModelUtils.Weapons.HeavyRifle);
            ped.GiveWeapon(ModelUtils.Weapons.StunGun);
            ped.GiveWeapon(ModelUtils.Weapons.Shotgun);
            return ped;
        }
    }
}