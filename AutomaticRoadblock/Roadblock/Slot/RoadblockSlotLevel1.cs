using System;
using System.Linq;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public class RoadblockSlotLevel1 : AbstractRoadblockSlot
    {
        internal RoadblockSlotLevel1(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
            : base(lane, heading, targetVehicle, shouldAddLights)
        {
        }

        public override void Spawn()
        {
            base.Spawn();
            CopInstances
                .Select(x => x.GameInstance)
                .ToList()
                .ForEach(x => x.WarpIntoVehicle(Vehicle, (int)VehicleSeat.Driver));
        }

        /// <inheritdoc />
        protected override Model GetVehicleModel()
        {
            return ModelUtils.GetLocalPoliceVehicle(Position, true, false);
        }

        protected override void InitializeCopPeds()
        {
            Instances.Add(new InstanceSlot(EntityType.CopPed, Position, 0f, (position, _) =>
                AssignCopWeapons(new ARPed(GetPedModelForVehicle(), position))));
        }

        protected override void InitializeScenery()
        {
            var rowPosition = Position + MathHelper.ConvertHeadingToDirection(Heading - 180) * 3f;
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(Heading + 90) * 2.5f;
            var direction = MathHelper.ConvertHeadingToDirection(Heading - 90);
            var totalCones = (int)Math.Ceiling(Lane.Width / 1.5f);

            for (var i = 0; i < totalCones; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.Scenery, startPosition, Heading,
                    (position, heading) => new ARScenery(PropUtils.CreateSmallConeWithStripes(position))));
                startPosition += direction * 1.5f;
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
            return ped;
        }
    }
}