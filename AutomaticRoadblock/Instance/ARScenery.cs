using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Utils;
using JetBrains.Annotations;
using Rage;

namespace AutomaticRoadblocks.Instance
{
    /// <summary>
    /// A ped which is controlled by the Automatic Roadblock plugin.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ARScenery : IARInstance<Entity>
    {
        public ARScenery(Entity gameInstance)
        {
            GameInstance = gameInstance;
        }

        /// <inheritdoc />
        [CanBeNull]
        public Entity GameInstance { get; }

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