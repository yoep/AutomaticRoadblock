using System.Collections.Generic;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Pursuit.Menu
{
    /// <summary>
    /// Dispatch spawn option for debugging which will spawn in a roadblock as it would during a pursuit.
    /// This will target the player vehicle instead of an actual suspect.
    /// </summary>
    public class PursuitDispatchSpawnComponentItem : IMenuComponent<UIMenuListItem>
    {
        private readonly IGame _game;
        private readonly IPursuitManager _pursuitManager;

        public PursuitDispatchSpawnComponentItem(IPursuitManager pursuitManager, IGame game)
        {
            _pursuitManager = pursuitManager;
            _game = game;
        }

        /// <inheritdoc />
        public UIMenuListItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.DispatchSpawn, AutomaticRoadblocksPlugin.DispatchSpawnDescription,
            new List<IDisplayItem>
            {
                new DisplayItem(DispatchSpawnType.Calculate, AutomaticRoadblocksPlugin.DispatchPreviewCalculateType),
                new DisplayItem(DispatchSpawnType.CurrentLocation, AutomaticRoadblocksPlugin.DispatchPreviewCurrentLocationType)
            });

        /// <inheritdoc />
        public MenuType Type => MenuType.Pursuit;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            var selectedValue = MenuItem.SelectedItem.Value;
            if (selectedValue.GetType() != typeof(DispatchSpawnType))
                return;

            _game.NewSafeFiber(() => _pursuitManager.DispatchNow(false, true, (DispatchSpawnType)selectedValue == DispatchSpawnType.CurrentLocation),
                "DispatchSpawnComponent.OnMenuActivation");
        }

        private enum DispatchSpawnType
        {
            Calculate,
            CurrentLocation
        }
    }
}