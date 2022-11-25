using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Type;
using Rage;

namespace AutomaticRoadblocks.Instances
{
    /// <summary>
    /// A vehicle which is controlled by the Automatic Roadblock plugin.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ARVehicle : IARInstance<Vehicle>
    {
        public ARVehicle(Vehicle instance, float heading = 0f, bool recordCollisions = false)
        {
            Assert.NotNull(instance, "instance cannot be null");
            GameInstance = instance;
            GameInstance.Heading = heading;
            GameInstance.IsPersistent = true;
            GameInstance.NeedsCollision = true;
            GameInstance.IsRecordingCollisions = recordCollisions;
            GameInstance.IsEngineOn = true;
            GameInstance.IsSirenOn = true;
            EntityUtils.VehicleLights(GameInstance, EVehicleLightState.AlwaysOn);
        }

        #region Properties

        /// <inheritdoc />
        public EEntityType Type => EEntityType.CopVehicle;

        /// <inheritdoc />
        public Vehicle GameInstance { get; }

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

        /// <summary>
        /// The vehicle model.
        /// </summary>
        public Model Model => GameInstance.Model;

        /// <inheritdoc />
        public bool IsInvalid => GameInstance == null ||
                                 !GameInstance.IsValid();

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive { get; private set; }

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
            PreviewUtils.TransformToNormal(GameInstance);
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            DeletePreview();
            EntityUtils.Remove(GameInstance);
        }

        #endregion

        #region IARInstance

        /// <inheritdoc />
        public void Release()
        {
            if (IsInvalid)
                return;
            
            GameInstance.IsPersistent = false;
            GameInstance.IsRecordingCollisions = false;
        }

        #endregion
    }
}