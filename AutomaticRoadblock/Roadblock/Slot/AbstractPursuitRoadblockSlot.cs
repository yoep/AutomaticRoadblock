using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Street.Info;
using Rage;
using VehicleType = AutomaticRoadblocks.Vehicles.VehicleType;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public abstract class AbstractPursuitRoadblockSlot : AbstractRoadblockSlot, IPursuitRoadblockSlot
    {
        protected AbstractPursuitRoadblockSlot(Road.Lane lane, BarrierType barrierType, VehicleType vehicleType, float heading, Vehicle targetVehicle,
            bool shouldAddLights)
            : base(lane, barrierType, vehicleType, heading, shouldAddLights, true)
        {
            Assert.NotNull(targetVehicle, "targetVehicle cannot be null");
            TargetVehicle = targetVehicle;

            Initialize();
        }

        #region Properties

        /// <summary>
        /// Get the target vehicle of the slot.
        /// </summary>
        protected Vehicle TargetVehicle { get; }

        #endregion

        #region IPursuitRoadblockSlot

        /// <inheritdoc />
        public bool HasCopBeenKilledByTarget => Instances.Where(x => x.Type == EEntityType.CopPed)
            .Select(x => (ARPed)x.Instance)
            .Where(x => !x.IsInvalid)
            .Select(x => x.GameInstance)
            .Where(x => x.IsDead)
            .Where(HasCopReceivedDamageFromVehicleOrSuspects)
            .Any(HasCopBeenKilledBySuspects);

        #endregion

        #region Functions

        private bool HasCopBeenKilledBySuspects(Entity cop)
        {
            if (TargetVehicle == null || !TargetVehicle.IsValid())
                return false;

            return cop.HasBeenDamagedBy(TargetVehicle) || TargetVehicle.Occupants.Any(cop.HasBeenDamagedBy);
        }

        private static bool HasCopReceivedDamageFromVehicleOrSuspects(Entity x)
        {
            return x.HasBeenDamagedByAnyVehicle || x.HasBeenDamagedByAnyPed;
        }

        #endregion
    }
}