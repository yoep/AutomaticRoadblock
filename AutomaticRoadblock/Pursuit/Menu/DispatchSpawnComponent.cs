using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Pursuit.Menu
{
    public class DispatchSpawnComponent : IMenuComponent<UIMenuItem>
    {
        private readonly IPursuitManager _pursuitManager;

        public DispatchSpawnComponent(IPursuitManager pursuitManager)
        {
            _pursuitManager = pursuitManager;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.DispatchSpawn, AutomaticRoadblocksPlugin.DispatchSpawnDescription);

        /// <inheritdoc />
        public MenuType Type => MenuType.PURSUIT;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _pursuitManager.DispatchNow(true);
        }
    }
}