using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Instances
{
    /// <summary>
    /// A vehicle which is controlled by the Automatic Roadblock plugin.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ARVehicle : AbstractInstance<Vehicle>
    {
        public ARVehicle(Vehicle instance, float heading = 0f, bool recordCollisions = false)
            : base(instance)
        {
            Assert.NotNull(instance, "instance cannot be null");
            GameInstance.Heading = heading;
            GameInstance.NeedsCollision = true;
            GameInstance.IsRecordingCollisions = recordCollisions;
            GameInstance.IsEngineOn = true;
            GameInstance.IsSirenOn = true;
        }

        #region Properties

        /// <inheritdoc />
        public override EEntityType Type => EEntityType.CopVehicle;

        /// <summary>
        /// The vehicle model.
        /// </summary>
        public Model Model => GameInstance.Model;

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
        }

        #endregion

        #region IARInstance

        /// <inheritdoc />
        public override void Release()
        {
            base.Release();
            if (IsInvalid)
                return;

            GameInstance.IsRecordingCollisions = false;
            Logger.Trace($"{GetType()} release state: {this}");
        }

        #endregion

        public override string ToString()
        {
            return $"{nameof(GameInstance.Heading)}: {GameInstance.Heading}, " +
                   $"{nameof(GameInstance.IsPersistent)}: {GameInstance.IsPersistent}, " +
                   $"{nameof(GameInstance.IsRecordingCollisions)}: {GameInstance.IsRecordingCollisions}, " +
                   $"{nameof(GameInstance.IsEngineOn)}: {GameInstance.IsEngineOn}, " +
                   $"{nameof(GameInstance.IsSirenOn)}: {GameInstance.IsSirenOn}";
        }
    }
}