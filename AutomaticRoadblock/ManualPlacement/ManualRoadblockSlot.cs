using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;

namespace AutomaticRoadblocks.ManualPlacement
{
    /// <remarks>Never use 0 cops as the LSPDFR function has some really weird behavior in this case.</remarks>
    public class ManualRoadblockSlot : AbstractRoadblockSlot
    {
        public ManualRoadblockSlot(Road.Lane lane, BarrierModel mainBarrier, BarrierModel secondaryBarrier, EBackupUnit backupType,
            List<LightModel> lightSources, float heading, bool shouldAddLights, bool copsEnabled, float offset)
            : base(lane, mainBarrier, secondaryBarrier, backupType, heading, shouldAddLights, false, offset)
        {
            Assert.NotNull(lightSources, "lightSources cannot be null");
            LightSources = lightSources;
            CopsEnabled = copsEnabled;

            Initialize();
        }

        #region Properties

        /// <summary>
        /// The light sources of the slot.
        /// </summary>
        public List<LightModel> LightSources { get; }

        /// <summary>
        /// Indicates if cops are enabled for the slot.
        /// </summary>
        private bool CopsEnabled { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Spawn()
        {
            base.Spawn();
            CopInstances
                .ToList()
                .ForEach(x => x.StandStill(180000));
        }

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

        protected override void InitializeCops(IEnumerable<ARPed> cops)
        {
            if (CopsEnabled)
            {
                base.InitializeCops(cops);
            }
            else
            {
                Logger.Trace("No cops wanted for the manual roadblock, disposing the cop instances");
                foreach (var cop in cops)
                {
                    cop.Dispose();
                }
            }
        }

        #endregion
    }
}