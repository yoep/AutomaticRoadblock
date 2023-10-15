using System.Diagnostics;
using System.Drawing;
using AutomaticRoadblocks.Logging;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Instances
{
    public abstract class AbstractInstance<T> : IARInstance<T> where T : Entity
    {
        protected readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        private InstanceState _state = InstanceState.Idle;

        protected AbstractInstance(T instance)
        {
            Assert.NotNull(instance, "instance cannot be null");
            GameInstance = instance;
            MakePersistent();
            DrawDebugInfo();
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

        /// <inheritdoc />
        public InstanceState State
        {
            get => _state;
            protected set => UpdateState(value);
        }

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
            State = InstanceState.Disposed;

            if (!IsInvalid)
                EntityUtils.Remove(GameInstance);
        }

        /// <inheritdoc />
        public virtual void Release()
        {
            // The default action of Release is the same as Dismiss
            // This however might be overridden to have separate behaviors for different instance implementations.
            Dismiss();
        }

        /// <inheritdoc />
        public void Dismiss()
        {
            State = InstanceState.Released;
            
            if (IsInvalid)
            {
                Logger.Warn($"Unable to dismiss instance {this}, game instance is invalid");
                return;
            }
            
            GameInstance.IsPersistent = false;
            GameInstance.Dismiss();
        }

        /// <inheritdoc />
        public void MakePersistent()
        {
            if (IsInvalid)
                return;

            if (State == InstanceState.Disposed)
            {
                Logger.Warn($"Unable to make instance persistent, invalid state {State} for {this}");
                return;
            }
            
            GameInstance.IsPersistent = true;
            State = InstanceState.Idle;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, " +
                   $"{nameof(IsInvalid)}: {IsInvalid}, " +
                   $"{nameof(State)}: {State}, " +
                   $"{nameof(IsPreviewActive)}: {IsPreviewActive}";
        }

        #endregion

        #region Functions

        private void UpdateState(InstanceState newState)
        {
            if (_state == InstanceState.Disposed)
            {
                Logger.Warn($"Unable to update instance state to {newState}, current state is {_state}");
                return;
            }

            _state = newState;
        }

        [Conditional("DEBUG")]
        private void DrawDebugInfo()
        {
            GameUtils.NewSafeFiber(() =>
            {
                while (State != InstanceState.Disposed && !IsInvalid)
                {
                    var position = Position + Vector3.WorldUp * 5f;
                    var color = State switch
                    {
                        InstanceState.Disposed => Color.Black,
                        InstanceState.Idle => GameInstance.IsPersistent ? Color.Orange : Color.Gray,
                        InstanceState.Released => Color.Green,
                    };

                    GameUtils.DrawArrow(position, Vector3.WorldDown, Rotator.Zero, 1.5f, color);
                    GameFiber.Yield();
                }
            }, "AbstractInstance.DrawDebugInfo");
        }

        #endregion
    }
}