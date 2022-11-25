using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Instances
{
    /// <summary>
    /// A ped which is controlled by the Automatic Roadblock plugin.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ARScenery : AbstractInstance<Entity>
    {
        public ARScenery(Entity instance)
            : base(instance)
        {
            if (!IsInvalid)
                GameInstance.IsPersistent = true;
        }

        #region Properties

        /// <inheritdoc />
        public override EEntityType Type => EEntityType.Scenery;

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public override void CreatePreview()
        {
            if (IsPreviewActive || IsInvalid)
                return;

            IsPreviewActive = true;
            PreviewUtils.TransformToPreview(GameInstance);
        }

        /// <inheritdoc />
        public override void DeletePreview()
        {
            if (!IsPreviewActive || IsInvalid)
                return;

            IsPreviewActive = false;
            PreviewUtils.TransformToNormal(GameInstance);
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            DeletePreview();
            EntityUtils.Remove(GameInstance);
        }

        #endregion

        #region IARInstance

        /// <inheritdoc />
        public override void Release()
        {
            base.Release();
            if (GameInstance == null || !GameInstance.IsValid())
                return;

            GameInstance.IsPersistent = false;
        }

        #endregion
    }
}