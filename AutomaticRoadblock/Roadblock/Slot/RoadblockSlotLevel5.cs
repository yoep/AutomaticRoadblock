using System;
using System.Linq;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public class RoadblockSlotLevel5 : AbstractRoadblockSlot
    {
        internal RoadblockSlotLevel5(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
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
            return ModelUtils.GetSwatPoliceVehicle();
        }

        protected override void InitializeCopPeds()
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

        private static ARPed AssignCopWeapons(ARPed ped)
        {
            ped.GivePrimaryWeapon(ModelUtils.Weapons.HeavyRifle);
            ped.GiveWeapon(ModelUtils.Weapons.StunGun);
            ped.GiveWeapon(ModelUtils.Weapons.Shotgun);
            return ped;
        }
    }
}