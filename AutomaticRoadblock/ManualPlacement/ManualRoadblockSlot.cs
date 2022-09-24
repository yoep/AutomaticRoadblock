using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class ManualRoadblockSlot : AbstractRoadblockSlot
    {
        public ManualRoadblockSlot(Road.Lane lane, BarrierModel mainBarrier, BarrierModel secondaryBarrier, EBackupUnit backupType,
            List<LightModel> lightSources, float heading, bool shouldAddLights, bool copsEnabled, float offset)
            : base(lane, mainBarrier, secondaryBarrier, backupType, heading, shouldAddLights, false, offset)
        {
            Assert.NotNull(lightSources, "lightSources cannot be null");
            LightSources = lightSources;

            if (!copsEnabled)
                NumberOfCops = 0;

            Initialize();
        }

        #region Properties

        /// <summary>
        /// The light sources of the slot.
        /// </summary>
        public List<LightModel> LightSources { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override string ToString()
        {
            return "{" +
                   $"{base.ToString()}, {nameof(BackupType)}: {BackupType}, {nameof(LightSources)}: [{string.Join(", ", LightSources)}]" +
                   "}";
        }

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override void InitializeScenery()
        {
            // no-op
        }

        /// <inheritdoc />
        protected override void InitializeLights()
        {
            Logger.Trace("Initializing the manual roadblock slot lights");
            Instances.AddRange(LightSources
                .Where(x => (x.Light.Flags & (ELightSourceFlags.RoadLeft | ELightSourceFlags.RoadRight)) == 0)
                .SelectMany(x => LightSourceFactory.Create(x, this))
                .ToList());
        }

        #endregion
    }
}