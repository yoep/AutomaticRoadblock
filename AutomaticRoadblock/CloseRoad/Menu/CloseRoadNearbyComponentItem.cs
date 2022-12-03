using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.CloseRoad.Menu
{
    public class CloseRoadNearbyComponentItem : IMenuComponent<UIMenuItem>
    {
        private readonly IGame _game;
        private readonly ICloseRoadDispatcher _closeRoadDispatcher;
        private readonly ILocalizer _localizer;

        private bool _isClosed;

        public CloseRoadNearbyComponentItem(IGame game, ICloseRoadDispatcher closeRoadDispatcher, ILocalizer localizer)
        {
            _game = game;
            _closeRoadDispatcher = closeRoadDispatcher;
            _localizer = localizer;

            MenuItem = new UIMenuItem(localizer[LocalizationKey.CloseNearbyRoad], localizer[LocalizationKey.CloseNearbyRoadDescription]);
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; }

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
            MenuItem.Text = _localizer[LocalizationKey.OpenClosedRoad];
            MenuItem.Description = _localizer[LocalizationKey.OpenClosedRoadDescription];
            _game.NewSafeFiber(() => _closeRoadDispatcher.CloseNearbyRoad(_game.PlayerPosition), $"{GetType()}.CloseNearbyRoad");
        }

        private void OpenNearbyRoad()
        {
            _isClosed = false;
            _closeRoadDispatcher.OpenRoads();
            MenuItem.Text = _localizer[LocalizationKey.CloseNearbyRoad];
            MenuItem.Description = _localizer[LocalizationKey.CloseNearbyRoadDescription];
        }
    }
}