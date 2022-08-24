using System.Linq;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using AutomaticRoadblocks.Utils.Type;
using Rage;

namespace AutomaticRoadblocks.Pursuit.Level
{
    public class PursuitRoadblockSlotLevel1 : AbstractPursuitRoadblockSlot
    {
        internal PursuitRoadblockSlotLevel1(Road.Lane lane, BarrierType barrierType, float heading, Vehicle targetVehicle, bool shouldAddLights)
            : base(lane, barrierType, heading, targetVehicle, shouldAddLights)
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
            return ModelUtils.Vehicles.GetLocalPoliceVehicle(Position, true, false);
        }

        protected override void InitializeCopPeds()
        {
            Instances.Add(new InstanceSlot(EntityType.CopPed, Position, 0f, (position, _) =>
                AssignCopWeapons(new ARPed(GetPedModelForVehicle(), position))));
        }

        protected override void InitializeScenery()
        {
            // no-op
        }

        protected override void InitializeLights()
        {
            Instances.AddRange(LightSourceSlotFactory.CreateFlares(this));
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