using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Street.Info;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class ManualRoadblock : AbstractRoadblock, IPlaceableInstance
    {
        internal ManualRoadblock(Request request)
            : base(request.Road, request.MainBarrier, request.SecondaryBarrier, request.TargetHeading, request.LightSources, RequestToFlags(request),
                request.Offset)
        {
            Assert.NotNull(request.BackupType, "vehicleType cannot be null");
            BackupType = request.BackupType;
            PlacementType = request.PlacementType;
            CopsEnabled = request.CopsEnabled;

            Initialize();
        }

        #region Properties

        /// <summary>
        /// The vehicle type used within the roadblock.
        /// </summary>
        public EBackupUnit BackupType { get; }

        /// <summary>
        /// The placement type of the roadblock.
        /// </summary>
        public PlacementType PlacementType { get; }

        /// <summary>
        /// Set if cops should be added.
        /// </summary>
        public bool CopsEnabled { get; }

        #endregion

        #region IRoadblock

        /// <inheritdoc />
        public override ERoadblockLevel Level => ERoadblockLevel.None;

        #endregion

        #region Methods

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{nameof(BackupType)}: {BackupType}, {nameof(PlacementType)}: {PlacementType}, {nameof(CopsEnabled)}: {CopsEnabled}\n" +
                $"{nameof(Slots)}: [{{{string.Join("}{,\n", Slots)}}}]";
        }

        #endregion

        #region Funtions

        /// <inheritdoc />
        protected override IReadOnlyList<IRoadblockSlot> CreateRoadblockSlots(IReadOnlyList<Road.Lane> lanesToBlock)
        {
            // check which lane(s) we need to block
            if (PlacementType == PlacementType.ClosestToPlayer)
            {
                lanesToBlock = new List<Road.Lane> { Road.LaneClosestTo(Game.PlayerPosition) };
            }
            else if (PlacementType == PlacementType.SameDirectionAsPlayer)
            {
                lanesToBlock = Road.LanesHeadingTo(Heading).ToList();
            }

            return lanesToBlock
                .Select(lane => new ManualRoadblockSlot(lane, MainBarrier, SecondaryBarrier, BackupType, LightSources, TargetHeading,
                    Flags.HasFlag(ERoadblockFlags.EnableLights), CopsEnabled, Offset))
                .ToList();
        }

        /// <inheritdoc />
        protected override void InitializeScenery()
        {
            Logger.Trace("Initializing the manual roadblock scenery items");
        }

        /// <inheritdoc />
        protected override void InitializeLights()
        {
            Logger.Trace("Initializing the manual roadblock lights");
            Instances.AddRange(LightSources
                .Where(x => (x.Light.Flags & ELightSourceFlags.Lane) == 0)
                .SelectMany(x => LightSourceFactory.Create(x, this)));
        }

        private static ERoadblockFlags RequestToFlags(Request request)
        {
            var flags = ERoadblockFlags.None;

            if (request.LimitSpeed)
                flags |= ERoadblockFlags.LimitSpeed;
            if (request.AddLights)
                flags |= ERoadblockFlags.EnableLights;

            return flags;
        }

        #endregion

        /// <summary>
        /// A simple wrapper class for creating a new <see cref="ManualRoadblock"/> to prevent constructor param mismatches.
        /// </summary>
        public class Request
        {
            public Road Road { get; set; }
            public BarrierModel MainBarrier { get; set; }
            public BarrierModel SecondaryBarrier { get; set; }
            public EBackupUnit BackupType { get; set; }
            public PlacementType PlacementType { get; set; }
            public float TargetHeading { get; set; }
            public bool LimitSpeed { get; set; }
            public bool AddLights { get; set; }
            public bool CopsEnabled { get; set; }
            public float Offset { get; set; }
            public List<LightModel> LightSources { get; set; }

            public override string ToString()
            {
                return
                    $"{nameof(Road)}: {Road}, {nameof(MainBarrier)}: {MainBarrier}, {nameof(SecondaryBarrier)}: {SecondaryBarrier}, " +
                    $"{nameof(BackupType)}: {BackupType}, {nameof(PlacementType)}: {PlacementType}, " +
                    $"{nameof(TargetHeading)}: {TargetHeading}, {nameof(LimitSpeed)}: {LimitSpeed}, {nameof(AddLights)}: {AddLights}, " +
                    $"{nameof(CopsEnabled)}: {CopsEnabled}, {nameof(Offset)}: {Offset}, {nameof(LightSources)}: {LightSources}";
            }
        }
    }
}