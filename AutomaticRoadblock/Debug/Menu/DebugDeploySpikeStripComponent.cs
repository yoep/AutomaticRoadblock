using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.SpikeStrip;
using AutomaticRoadblocks.SpikeStrip.Dispatcher;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugDeploySpikeStripComponent : IMenuComponent<UIMenuListScrollerItem<ESpikeStripLocation>>
    {
        private static readonly IGame Game = IoC.Instance.GetInstance<IGame>();

        private readonly ISpikeStripDispatcher _spikeStripDispatcher;

        private bool _deployed;

        public DebugDeploySpikeStripComponent(ISpikeStripDispatcher spikeStripDispatcher)
        {
            _spikeStripDispatcher = spikeStripDispatcher;
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<ESpikeStripLocation> MenuItem { get; } = new(AutomaticRoadblocksPlugin.DeploySpikeStrip, AutomaticRoadblocksPlugin.DeploySpikeStripDescription, new []
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
            _spikeStripDispatcher.Deploy(Game.PlayerPosition, MenuItem.SelectedItem);
            MenuItem.Text = AutomaticRoadblocksPlugin.RemoveSpikeStrip;
        }

        private void RemoveSpikeStrip()
        {
            _deployed = false;
            _spikeStripDispatcher.RemoveAll();
            MenuItem.Text = AutomaticRoadblocksPlugin.DeploySpikeStrip;
        }
    }
}