using AutomaticRoadblocks.SpikeStrip;
using Rage;

namespace AutomaticRoadblocks.Instances
{
    public class ARSpikeStrip : IARInstance<Object>
    {
        public ARSpikeStrip(ISpikeStrip spikeStrip)
        {
            SpikeStrip = spikeStrip;
        }

        #region Properties

        /// <inheritdoc />
        public EEntityType Type => EEntityType.SpikeStrip;

        /// <inheritdoc />
        public Object GameInstance => SpikeStrip?.GameInstance;

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
        public bool IsInvalid => SpikeStrip.State is ESpikeStripState.Preparing or ESpikeStripState.Disposed;

        /// <inheritdoc />
        public InstanceState State { get; private set; }

        /// <summary>
        /// The spike strip instance.
        /// </summary>
        public ISpikeStrip SpikeStrip { get; }

        #endregion

        /// <inheritdoc />
        public void Release()
        {
            Dismiss();
        }

        /// <inheritdoc />
        public void Dismiss()
        {
            // retract the spike strip
            SpikeStrip.Undeploy();
            State = InstanceState.Released;
        }

        /// <inheritdoc />
        public void MakePersistent()
        {
            // no-op
        }

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => SpikeStrip.IsPreviewActive;

        /// <inheritdoc />
        public void CreatePreview()
        {
            SpikeStrip.CreatePreview();
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            SpikeStrip.DeletePreview();
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            SpikeStrip.Dispose();
            State = InstanceState.Disposed;
        }

        #endregion
    }
}