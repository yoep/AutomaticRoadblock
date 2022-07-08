using Rage;
using Rage.Native;

namespace AutomaticRoadblock.Utils
{
    public static class PreviewUtils
    {
        /// <summary>
        /// Transforms the given entity to a preview entity.
        /// </summary>
        /// <param name="entity">Set the entity to transform.</param>
        public static void TransformToPreview(Entity entity)
        {
            Assert.NotNull(entity, "entity cannot be null");
            entity.Opacity = 0.7f;
            entity.IsPositionFrozen = true;
            entity.NeedsCollision = false;
            entity.IsCollisionEnabled = false;
            NativeFunction.Natives.SET_ENTITY_COLLISION(entity, false, false);
        }
    }
}