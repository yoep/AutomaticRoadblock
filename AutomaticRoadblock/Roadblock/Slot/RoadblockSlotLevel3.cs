using System.Linq;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public class RoadblockSlotLevel3 : AbstractRoadblockSlot
    {
        internal RoadblockSlotLevel3(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
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
                    x.GameInstance.Tasks.TakeCoverFrom(TargetVehicle.Driver, -1, true);
                });
        }

        protected override Model GetVehicleModel()
        {
            return Random.Next(3) == 0 ? ModelUtils.Vehicles.GetLocalPoliceVehicle(Position, false) : ModelUtils.Vehicles.GetStatePoliceVehicle(false);
        }

        protected override void InitializeCopPeds()
        {
            var pedSpawnPosition = GetPositionBehindVehicle();

            for (var i = 0; i < 2; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.CopPed, pedSpawnPosition, 0f,
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
        }

        private ARPed AssignCopWeapons(ARPed ped)
        {
            var primaryWeapon = Random.Next(2) == 1 ? ModelUtils.Weapons.Shotgun : ModelUtils.Weapons.Pistol;

            ped.GivePrimaryWeapon(primaryWeapon);
            ped.GiveWeapon(ModelUtils.Weapons.Pistol);
            ped.GiveWeapon(ModelUtils.Weapons.Shotgun);
            ped.GiveWeapon(ModelUtils.Weapons.Nightstick);
            ped.GiveWeapon(ModelUtils.Weapons.StunGun);
            return ped;
        }
    }
}