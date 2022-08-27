using System.Linq;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public abstract class AbstractPursuitRoadblockSlot : AbstractRoadblockSlot, IPursuitRoadblockSlot
    {
        protected AbstractPursuitRoadblockSlot(Road.Lane lane, BarrierType barrierType, float heading, Vehicle targetVehicle, bool shouldAddLights)
            : base(lane, barrierType, heading, shouldAddLights, true)
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
        public bool HasCopBeenKilledByTarget => Instances.Where(x => x.Type == EntityType.CopPed)
            .Select(x => (ARPed)x.Instance)
            .Where(IsGameInstanceValid)
            .Select(x => x.GameInstance)
            .Where(x => x.IsDead)
            .Where(HasCopReceivedDamageFromVehicleOrSuspects)
            .Any(HasCopBeenKilledBySuspects);

        #endregion

        #region Functions

        private bool HasCopBeenKilledBySuspects(Entity cop)
        {
            return cop.HasBeenDamagedBy(TargetVehicle) || TargetVehicle.Occupants.Any(cop.HasBeenDamagedBy);
        }

        private static bool IsGameInstanceValid(ARPed entity)
        {
            return entity.GameInstance != null && entity.GameInstance.IsValid();
        }

        private static bool HasCopReceivedDamageFromVehicleOrSuspects(Entity x)
        {
            return x.HasBeenDamagedByAnyVehicle || x.HasBeenDamagedByAnyPed;
        }

        #endregion
    }
}