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
        public Object GameInstance => null;

        /// <inheritdoc />
        public bool IsInvalid => SpikeStrip.State == ESpikeStripState.Disposed;

        /// <summary>
        /// The spike strip instance.
        /// </summary>
        private ISpikeStrip SpikeStrip { get; }

        #endregion

        /// <inheritdoc />
        public void Release()
        {
            // retract
            SpikeStrip.Undeploy();
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
        }

        #endregion
    }
}