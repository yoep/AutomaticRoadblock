using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Barriers
{
    public static class BarrierFactory
    {
        public static ARScenery Create(BarrierModel barrierModel, Vector3 position, float heading = 0f)
        {
            Assert.NotNull(barrierModel, "barrierModel cannot be null");
            Assert.NotNull(position, "position cannot be null");
            var groundPostPosition = GameUtils.GetOnTheGroundPosition(position);
            return new ARScenery(new Object(barrierModel.Model, groundPostPosition, heading));
        }
    }
}