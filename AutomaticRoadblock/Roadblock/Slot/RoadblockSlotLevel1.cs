using System.Linq;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public class RoadblockSlotLevel1 : AbstractRoadblockSlot
    {
        internal RoadblockSlotLevel1(Vector3 position, float heading, Vehicle targetVehicle) : base(position, heading, targetVehicle)
        {
            Init();
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

        private void Init()
        {
            InitializeVehicleSlot();
            InitializePedSlots();
            InitializeCones();
        }

        private void InitializePedSlots()
        {
            var isBike = ModelUtils.IsBike(VehicleModel);
            Instances.Add(new InstanceSlot(EntityType.CopPed, Position, 0f, (position, heading) =>
                isBike
                    ? AssignCopWeapons(new ARPed(ModelUtils.GetPoliceBikeCop(), Position))
                    : AssignCopWeapons(new ARPed(ModelUtils.GetLocalCop(Position), Position))));
        }

        private void InitializeCones()
        {
            var rowPosition = Position + MathHelper.ConvertHeadingToDirection(Heading - 180) * 3f;
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(Heading + 90) * 2.5f;

            for (var i = 0; i < 5; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.Cone, startPosition, Heading,
                    (position, heading) => new ARScenery(PropUtils.CreateSmallConeWithStripes(position))));
                startPosition += MathHelper.ConvertHeadingToDirection(Heading - 90) * 1.5f;
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