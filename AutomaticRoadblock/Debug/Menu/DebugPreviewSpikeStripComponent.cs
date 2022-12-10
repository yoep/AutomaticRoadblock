using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.SpikeStrip;
using AutomaticRoadblocks.SpikeStrip.Dispatcher;
using AutomaticRoadblocks.Utils;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugPreviewSpikeStripComponent : IMenuComponent<UIMenuListScrollerItem<ESpikeStripLocation>>
    {
        private readonly ISpikeStripDispatcher _spikeStripDispatcher;

        private bool _deployed;

        public DebugPreviewSpikeStripComponent(ISpikeStripDispatcher spikeStripDispatcher)
        {
            _spikeStripDispatcher = spikeStripDispatcher;
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<ESpikeStripLocation> MenuItem { get; } = new(AutomaticRoadblocksPlugin.PreviewSpikeStrip,
            AutomaticRoadblocksPlugin.PreviewSpikeStripDescription, new[]
            {
                ESpikeStripLocation.Left,
                ESpikeStripLocation.Middle,
                ESpikeStripLocation.Right
            });

        /// <inheritdoc />
        public EMenuType Type => EMenuType.Debug;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            if (!_deployed)
            {
                DeploySpikeStrip();
            }
            else
            {
                RemoveSpikeStrip();
            }
        }

        private void DeploySpikeStrip()
        {
            _deployed = true;
            _spikeStripDispatcher.CreatePreview(GameUtils.PlayerPosition, MenuItem.SelectedItem);
            MenuItem.Text = AutomaticRoadblocksPlugin.RemoveSpikeStripPreview;
        }

        private void RemoveSpikeStrip()
        {
            _deployed = false;
            _spikeStripDispatcher.RemoveAll();
            MenuItem.Text = AutomaticRoadblocksPlugin.PreviewSpikeStrip;
        }
    }
}