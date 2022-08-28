using System;
using System.Collections.Generic;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Barriers
{
    public static class BarrierFactory
    {
        private static readonly Dictionary<BarrierType, Func<Vector3, float, ARScenery>> Barriers =
            new()
            {
                { BarrierType.None, (_, _) => null },
                { BarrierType.SmallCone, (position, _) => new ARScenery(PropUtils.CreateSmallBlankCone(position)) },
                { BarrierType.SmallConeStriped, (position, _) => new ARScenery(PropUtils.CreateSmallConeWithStripes(position)) },
                { BarrierType.BigCone, (position, _) => new ARScenery(PropUtils.CreateBigCone(position)) },
                { BarrierType.BigConeStriped, (position, _) => new ARScenery(PropUtils.CreateBigConeWithStripes(position)) },
                { BarrierType.ConeWithLight, (position, heading) => new ARScenery(PropUtils.CreateConeWithLight(position, heading)) },
                { BarrierType.PoliceDoNotCross, (position, heading) => new ARScenery(PropUtils.CreatePoliceDoNotCrossBarrier(position, heading)) },
                { BarrierType.WorkBarrierLarge, (position, heading) => new ARScenery(PropUtils.CreateWorkBarrierLarge(position, heading)) },
                { BarrierType.WorkBarrierSmall, (position, heading) => new ARScenery(PropUtils.CreateWorkBarrierSmall(position, heading)) },
                { BarrierType.WorkBarrierWithSign, (position, heading) => new ARScenery(PropUtils.CreateBarrierWithWorkAhead(position, heading)) },
                {
                    BarrierType.WorkBarrierWithSignLight,
                    (position, heading) => new ARScenery(PropUtils.CreateBarrierWithWorkAheadWithLights(position, heading))
                },
                { BarrierType.WorkBarrierHigh, (position, heading) => new ARScenery(PropUtils.CreateWorkBarrierHigh(position, heading)) },
                { BarrierType.BarrelTrafficCatcher, (position, heading) => new ARScenery(PropUtils.CreateBarrelTrafficCatcher(position, heading)) },
            };

        public static ARScenery Create(BarrierType type, Vector3 position, float heading = 0f)
        {
            Assert.NotNull(type, "type cannot be null");
            Assert.NotNull(position, "position cannot be null");
            return Barriers[type].Invoke(GameUtils.GetOnTheGroundPosition(position), heading);
        }
    }
}