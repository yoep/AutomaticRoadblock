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

        internal ManualRoadblock(Road road, BarrierType barrierType, VehicleType vehicleType, LightSourceType lightSourceType, float targetHeading, bool limitSpeed, bool addLights)
            : base(road, barrierType, targetHeading, limitSpeed, addLights)
        {
            Assert.NotNull(vehicleType, "vehicleType cannot be null");
            Assert.NotNull(lightSourceType, "lightSourceType cannot be null");
            VehicleType = vehicleType;
            LightSourceType = lightSourceType;

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

        #endregion

        #region IRoadblock

        /// <inheritdoc />
        public override RoadblockLevel Level => RoadblockLevel.None;

        #endregion

        #region Funtions

        /// <inheritdoc />
        protected override IReadOnlyCollection<IRoadblockSlot> CreateRoadblockSlots(IReadOnlyList<Road.Lane> lanesToBlock)
        {
            return lanesToBlock
                .Select(lane => new ManualRoadblockSlot(lane, MainBarrierType, VehicleType, LightSourceType, TargetHeading, IsLightsEnabled))
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
    }
}