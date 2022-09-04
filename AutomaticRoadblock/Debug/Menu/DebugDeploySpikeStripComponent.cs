using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.SpikeStrip.Dispatcher;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugDeploySpikeStripComponent : IMenuComponent<UIMenuItem>
    {
        private static readonly IGame Game = IoC.Instance.GetInstance<IGame>();

        private readonly ISpikeStripDispatcher _spikeStripDispatcher;

        private bool _deployed;

        public DebugDeploySpikeStripComponent(ISpikeStripDispatcher spikeStripDispatcher)
        {
            _spikeStripDispatcher = spikeStripDispatcher;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.DeploySpikeStrip);

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
            _spikeStripDispatcher.Deploy(Game.PlayerPosition);
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