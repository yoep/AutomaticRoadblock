using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils.Road;

namespace AutomaticRoadblocks.ManualPlacement
{
    public class ManualRoadblock : AbstractRoadblock
    {
        internal ManualRoadblock(Request request)
            : base(request.Road, request.BarrierType, request.TargetHeading, request.LimitSpeed, request.AddLights)
        {
            Assert.NotNull(request.VehicleType, "vehicleType cannot be null");
            Assert.NotNull(request.LightSourceType, "lightSourceType cannot be null");
            VehicleType = request.VehicleType;
            LightSourceType = request.LightSourceType;
            PlacementType = request.PlacementType;
            CopsEnabled = request.CopsEnabled;

            Initialize();
        }

        #region Properties

        /// <summary>
        /// The vehicle type used within the roadblock.
        /// </summary>
        public VehicleType VehicleType { get; }

        /// <summary>
        /// The light type used within the roadblock.
        /// </summary>
        public LightSourceType LightSourceType { get; }

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
        public override RoadblockLevel Level => RoadblockLevel.None;

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
                .Select(lane => new ManualRoadblockSlot(lane, MainBarrierType, VehicleType, LightSourceType, TargetHeading, IsLightsEnabled, CopsEnabled))
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
            if (LightSourceType == LightSourceType.Spots)
            {
                Instances.AddRange(LightSourceRoadblockFactory.CreateGeneratorLights(this));
            }
        }

        #endregion

        /// <summary>
        /// A simple wrapper class for creating a new <see cref="ManualRoadblock"/> to prevent constructor param mismatches.
        /// </summary>
        public class Request
        {
            public Road Road { get; set; }
            public BarrierType BarrierType { get; set; }
            public VehicleType VehicleType { get; set; }
            public LightSourceType LightSourceType { get; set; }
            public PlacementType PlacementType { get; set; }
            public float TargetHeading { get; set; }
            public bool LimitSpeed { get; set; }
            public bool AddLights { get; set; }
            public bool CopsEnabled { get; set; }
        }
    }
}