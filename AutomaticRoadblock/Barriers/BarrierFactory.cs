using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Models;
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
            if (barrierModel.Model == null)
                throw new InvalidModelException(barrierModel);

            var groundPostPosition = GameUtils.GetOnTheGroundPosition(position);
            var instance = new Object(barrierModel.Model.Value, groundPostPosition, heading);
            instance.Position += Vector3.WorldUp * barrierModel.VerticalOffset;
            return new ARScenery(instance);
        }
    }
}