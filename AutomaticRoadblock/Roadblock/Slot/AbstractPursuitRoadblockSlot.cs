using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Street.Info;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public abstract class AbstractPursuitRoadblockSlot : AbstractRoadblockSlot, IPursuitRoadblockSlot
    {
        protected AbstractPursuitRoadblockSlot(Road.Lane lane, BarrierModel mainBarrier, BarrierModel secondaryBarrier, EBackupUnit backupType, float heading,
            Vehicle targetVehicle,
            List<LightModel> lightSources, bool shouldAddLights)
            : base(lane, mainBarrier, secondaryBarrier, backupType, heading, shouldAddLights, true)
        {
            Assert.NotNull(targetVehicle, "targetVehicle cannot be null");
            Assert.NotNull(lightSources, "lightSources cannot be null");
            TargetVehicle = targetVehicle;
            LightSources = lightSources;

            Initialize();
        }

        #region Properties

        /// <summary>
        /// Get the target vehicle of the slot.
        /// </summary>
        protected Vehicle TargetVehicle { get; }

        /// <summary>
        /// The light sources of this slot.
        /// </summary>
        protected List<LightModel> LightSources { get; }

        #endregion

        #region IPursuitRoadblockSlot

        /// <inheritdoc />
        public bool HasCopBeenKilledByTarget => Instances.Where(x => x.Type == EEntityType.CopPed)
            .Select(x => (ARPed)x)
            .Where(x => x is { IsInvalid: false })
            .Select(x => x.GameInstance)
            .Where(x => x.IsDead)
            .Where(HasCopReceivedDamageFromVehicleOrSuspects)
            .Any(HasCopBeenKilledBySuspects);

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override void InitializeLights()
        {
            Instances.AddRange(LightSources
                .SelectMany(x => LightSourceFactory.Create(x, this)));
        }

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