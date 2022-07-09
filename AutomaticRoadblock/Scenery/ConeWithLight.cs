using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Scenery
{
    public class ConeWithLight : AbstractPlaceableSceneryItem
    {
        public ConeWithLight(Vector3 position, float heading) : base(position, heading)
        {
        }

        protected override Object CreateItemInstance()
        {
            return PropUtils.CreateConeWithLight(Position, Heading);
        }
    }
}