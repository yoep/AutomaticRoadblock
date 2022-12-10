using System.Diagnostics;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Instances
{
    public abstract class AbstractInstance<T> : IARInstance<T> where T : Entity
    {
        protected readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        private bool _isReleased;

        protected AbstractInstance(T instance)
        {
            Assert.NotNull(instance, "instance cannot be null");
            GameInstance = instance;
            GameInstance.MakePersistent();
            MonitorState();
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
            _isReleased = true;

            if (!IsInvalid)
                EntityUtils.Remove(GameInstance);
        }

        /// <inheritdoc />
        public virtual void Release()
        {
            _isReleased = true;

            if (!IsInvalid)
            {
                GameInstance.IsPersistent = false;
                GameInstance.Dismiss();
            }
        }

        #endregion

        #region Functions

        private void MonitorState()
        {
            GameUtils.NewSafeFiber(() =>
            {
                while (!_isReleased)
                {
                    if (IsInvalid)
                    {
                        StateChanged(nameof(GameInstance.IsValid), GameInstance.IsValid());
                        break;
                    }

                    if (!_isReleased && !GameInstance.IsPersistent)
                    {
                        StateChanged(nameof(GameInstance.IsPersistent), GameInstance.IsPersistent);
                        GameInstance.MakePersistent();
                    }

                    GameFiber.Wait(1000);
                }
            }, $"{GetType()}.MonitorState");
        }

        [Conditional("DEBUG")]
        private void StateChanged(string field, object newValue)
        {
            Logger.Warn($"{GetType()}#{field} state changed to {newValue}");
            GameUtils.DisplayNotificationDebug($"~o~State changed of ~s~{GetType()}~c~#~b~{field}");
        }

        #endregion
    }
}