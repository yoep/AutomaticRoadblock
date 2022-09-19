using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Utils
{
    public static class PreviewUtils
    {
        /// <summary>
        /// Transforms the given entity to a preview entity.
        /// </summary>
        /// <param name="entity">Set the entity to transform.</param>
        public static void TransformToPreview(Entity entity)
        {
            SetToPreviewState(entity, true);
        }

        private static void SetToPreviewState(Entity entity, bool isPreview)
        {
            Assert.NotNull(entity, "entity cannot be null");
            entity.Opacity = isPreview ? 0.7f : 1f;
            entity.IsPositionFrozen = isPreview;
            entity.NeedsCollision = !isPreview;
            entity.IsCollisionEnabled = !isPreview;
            NativeFunction.Natives.SET_ENTITY_COLLISION(entity, !isPreview, !isPreview);

            if (entity.GetType() == typeof(Ped))
            {
                ((Ped)entity).BlockPermanentEvents = isPreview;
            }
        }
    }
}