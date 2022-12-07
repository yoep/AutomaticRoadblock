using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Animation;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;
using Rage;

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

        /// <summary>
        /// The cop which will play the redirect traffic animation.
        /// </summary>
        private ARPed RedirectTrafficCop { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Spawn()
        {
            base.Spawn();
            RedirectTrafficCop.RedirectTraffic();
            CopInstances
                .Where(x => x != RedirectTrafficCop)
                .ToList()
                .ForEach(x => AnimationHelper.PlayAnimation(x.GameInstance, Animations.Dictionaries.GuardDictionary, "idle_a", AnimationFlags.Loop));
        }

        /// <inheritdoc />
        public override void Release(bool releaseAll = false)
        {
            Cops.ToList().ForEach(x =>
            {
                x.DeleteAttachments();
                x.ClearAllTasks();
            });
            base.Release(releaseAll);
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

        /// <inheritdoc />
        protected override void InitializeCops(IEnumerable<ARPed> instances)
        {
            if (CopsEnabled)
            {
                var cops = instances.ToList();

                InitializeRedirectTrafficCop(cops.First());
                base.InitializeCops(cops.Skip(1));
            }
            else
            {
                Logger.Trace("No cops wanted for the manual roadblock, disposing the cop instances");
                foreach (var cop in instances)
                {
                    cop.Dispose();
                }
            }
        }

        /// <inheritdoc />
        protected override float CalculateCopHeading()
        {
            return Heading;
        }

        private void InitializeRedirectTrafficCop(ARPed cop)
        {
            cop.PlaceOnGroundAt(CalculatePositionLeftOfVehicle());
            cop.Heading = MathHelper.NormalizeHeading(CalculateCopHeading() + 180);
            Instances.Add(cop);
            RedirectTrafficCop = cop;
        }

        #endregion
    }
}