using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    public class RoadblockLevel1 : AbstractRoadblock
    {
        public RoadblockLevel1(Road road, Vehicle vehicle, bool limitSpeed, bool addLights)
            : base(road, vehicle, limitSpeed, addLights)
        {
        }

        #region Properties

        /// <inheritdoc />
        public override RoadblockLevel Level => RoadblockLevel.Level1;

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override void InitializeScenery()
        {
            var position = Postion + MathHelper.ConvertHeadingToDirection(MathHelper.NormalizeHeading(Road.Node.Heading - 180)) * 3f;

            Instances.Add(new InstanceSlot(EntityType.Scenery, position, 0f,
                (conePosition, _) => new ARScenery(PropUtils.CreateBigConeWithStripes(conePosition))));
        }

        /// <inheritdoc />
        protected override void InitializeLights()
        {
            // no-op
        }

        /// <inheritdoc />
        protected override IReadOnlyList<Road.Lane> LanesToBlock()
        {
            var lanesToBlock = base.LanesToBlock();

            return lanesToBlock
                .Where(x => Math.Abs(x.Heading - Vehicle.Heading) < LaneHeadingTolerance)
                .ToList();
        }

        /// <inheritdoc />
        protected override IRoadblockSlot CreateSlot(Road.Lane lane, float heading, Vehicle targetVehicle, bool shouldAddLights)
        {
            return new RoadblockSlotLevel1(lane, heading, targetVehicle, shouldAddLights);
        }

        #endregion
    }
}