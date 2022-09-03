using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Roadblock.Dispatcher;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Roadblock.Menu
{
    public class CleanRoadblocksComponent : IMenuComponent<UIMenuItem>
    {
        private readonly IRoadblockDispatcher _roadblockDispatcher;

        public CleanRoadblocksComponent(IRoadblockDispatcher roadblockDispatcher)
        {
            _roadblockDispatcher = roadblockDispatcher;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AutomaticRoadblocksPlugin.CleanAllRoadblocks);

        /// <inheritdoc />
        public EMenuType Type => EMenuType.Pursuit;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _roadblockDispatcher.Dispose();
        }
    }
}