using AutomaticRoadblocks.Logging;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Instances
{
    public abstract class AbstractInstance<T> : IARInstance<T> where T : Entity
    {
        protected readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        protected AbstractInstance(T instance)
        {
            Assert.NotNull(instance, "instance cannot be null");
            GameInstance = instance;
            GameInstance.MakePersistent();
        }

        #region Properties

        /// <inheritdoc />
        public T GameInstance { get; }

        /// <inheritdoc />
        public abstract EEntityType Type { get; }

        /// <inheritdoc />
        public Vector3 Position
        {
            get => GameInstance.Position;
            set => GameInstance.Position = value;
        }

        /// <inheritdoc />
        public float Heading
        {
            get => GameInstance.Heading;
            set => GameInstance.Heading = value;
        }

        /// <inheritdoc />
        public bool IsInvalid => GameInstance == null ||
                                 !GameInstance.IsValid();

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive { get; protected set; }

        /// <inheritdoc />
        public abstract void CreatePreview();

        /// <inheritdoc />
        public abstract void DeletePreview();

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual void Dispose()
        {
            if (!IsInvalid)
                EntityUtils.Remove(GameInstance);
        }

        /// <inheritdoc />
        public virtual void Release()
        {
            if (!IsInvalid)
            {
                GameInstance.IsPersistent = false;
                GameInstance.Dismiss();
            }
        }

        #endregion
    }
}