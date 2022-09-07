namespace AutomaticRoadblocks.SpikeStrip
{
    public class SpikeStripEvents
    {
        /// <summary>
        /// Invoked when the spikestrip state changed.
        /// </summary>
        public delegate void SpikeStripStateChanged(ISpikeStrip spikeStrip, ESpikeStripState newState);
    }
}