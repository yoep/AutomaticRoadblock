using System;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Instance
{
    public class InstanceSlot : IDisposable, IPreviewSupport
    {
        private readonly Func<Vector3, float, ARInstance<Entity>> _factory;

        public InstanceSlot(EntityType type, Vector3 position, float heading, Func<Vector3, float, ARInstance<Entity>> factory)
        {
            Assert.NotNull(type, "type cannot be null");
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(factory, "factory cannot be null");
            _factory = factory;
            Type = type;
            Position = position;
            Heading = heading;
        }

        /// <summary>
        /// Get the slot entity type.
        /// </summary>
        public EntityType Type { get; }

        /// <summary>
        /// Get the position of the entity.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Get the heading of the entity.
        /// </summary>
        public float Heading { get; }

        /// <summary>
        /// Get the instance if it's spawned.
        /// </summary>
        public ARInstance<Entity> Instance { get; private set; }

        /// <summary>
        /// Spawn the entity if it's not already spawned.
        /// </summary>
        public void Spawn()
        {
            if (Instance != null)
                return;

            Instance = _factory.Invoke(EntityUtils.OnGround(Position), Heading);
        }

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive { get; private set; }

        /// <inheritdoc />
        public void CreatePreview()
        {
            IsPreviewActive = true;
            Spawn();
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