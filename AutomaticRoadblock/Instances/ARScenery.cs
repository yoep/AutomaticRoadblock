using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Utils;
using JetBrains.Annotations;
using Rage;

namespace AutomaticRoadblocks.Instances
{
    /// <summary>
    /// A ped which is controlled by the Automatic Roadblock plugin.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ARScenery : IARInstance<Entity>
    {
        public ARScenery(Entity gameInstance)
        {
            Assert.NotNull(gameInstance, "gameInstance cannot be null");
            GameInstance = gameInstance;
            GameInstance.IsPersistent = true;
        }
        
        #region Properties

        /// <inheritdoc />
        [CanBeNull]
        public Entity GameInstance { get; }

        /// <inheritdoc />
        public bool IsInvalid => GameInstance == null ||
                                 !GameInstance.IsValid();

        #endregion
        
        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive { get; private set;  }

        /// <inheritdoc />
        public void CreatePreview()
        {
            if (IsPreviewActive || IsInvalid)
                return;

            IsPreviewActive = true;
            PreviewUtils.TransformToPreview(GameInstance);
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            if (!IsPreviewActive || IsInvalid)
                return;
            
            IsPreviewActive = false;
            EntityUtils.Remove(GameInstance);
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            EntityUtils.Remove(GameInstance);
        }

        #endregion
        
        #region IARInstance

        /// <inheritdoc />
        public void Release()
        {
            if (GameInstance == null || !GameInstance.IsValid())
                return;
            
            GameInstance.IsPersistent = false;
        }

        #endregion
    }
}