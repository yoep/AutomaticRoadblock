using System;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Instance
{
    public class InstanceSlot : IDisposable, IPreviewSupport
    {
        private readonly ILogger _logger = IoC.Instance.GetInstance<ILogger>();
        private readonly Func<Vector3, float, IARInstance<Entity>> _factory;

        public InstanceSlot(EntityType type, Vector3 position, float heading, Func<Vector3, float, IARInstance<Entity>> factory)
        {
            Assert.NotNull(type, "type cannot be null");
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(factory, "factory cannot be null");
            _factory = factory;
            Type = type;
            Position = position;
            Heading = heading;
        }

        #region Properties


        /// <summary>
        /// Get the slot entity type.
        /// </summary>
        public EntityType Type { get; }

        /// <summary>
        /// The position of the entity.
        /// </summary>
        /// <remarks>It's recommended to not change the position when the entity has been <see cref="Spawn"/></remarks>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Get the heading of the entity.
        /// </summary>
        public float Heading { get; }

        /// <summary>
        /// Get the instance if it's spawned.
        /// </summary>
        public IARInstance<Entity> Instance { get; private set; }

        /// <summary>
        /// The state of the instance slot.
        /// </summary>
        public InstanceState State { get; private set; } = InstanceState.Inactive;
        
        #endregion

        #region Methods
        
        /// <summary>
        /// Spawn the entity if it's not already spawned.
        /// </summary>
        public void Spawn()
        {
            if (Instance != null)
                return;

            State = InstanceState.Spawning;

            try
            {
                var instance = _factory.Invoke(Position, Heading);
                State = InstanceState.Spawned;

                if (instance != null)
                {
                    Instance = instance;
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create instance slot, {ex.Message}", ex);
                State = InstanceState.Error;
            }

            _logger.Warn($"Created a 'null' instance for {GetType().FullName}: {this}");
        }

        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, {nameof(Position)}: {Position}, {nameof(Heading)}: {Heading}";
        }

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive { get; private set; }

        /// <inheritdoc />
        public void CreatePreview()
        {
            IsPreviewActive = true;
            Spawn();

            // verify that an instance was created
            if (Instance != null)
                PreviewUtils.TransformToPreview(Instance.GameInstance);
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            Dispose();
            IsPreviewActive = false;
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            if (Instance == null)
                return;

            Instance.Dispose();
            Instance = null;
        }

        #endregion
    }
}