using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Utils;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.CloseRoad.Menu
{
    public class CloseRoadNearbyPreviewComponentItem : IMenuComponent<UIMenuItem>
    {
        private readonly ICloseRoadDispatcher _closeRoadDispatcher;

        private bool _isClosed;

        public CloseRoadNearbyPreviewComponentItem(ICloseRoadDispatcher closeRoadDispatcher)
        {
            _closeRoadDispatcher = closeRoadDispatcher;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.CloseNearbyRoad);

        /// <inheritdoc />
        public EMenuType Type => EMenuType.CloseRoad;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            if (!_isClosed)
            {
                CloseNearbyRoad();
            }
            else
            {
                OpenNearbyRoad();
            }
        }

        private void CloseNearbyRoad()
        {
            _isClosed = true;
            _closeRoadDispatcher.CloseNearbyRoad(GameUtils.PlayerPosition, true);
            MenuItem.Text = AutomaticRoadblocksPlugin.OpenNearbyRoad;
        }

        private void OpenNearbyRoad()
        {
            _isClosed = false;
            _closeRoadDispatcher.OpenRoads(true);
            MenuItem.Text = AutomaticRoadblocksPlugin.CloseNearbyRoad;
        }
    }
}