using System.Collections.Generic;
using AutomaticRoadblocks.Menu;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Pursuit.Menu
{
    /// <summary>
    /// Dispatch spawn option for debugging which will spawn in a roadblock as it would during a pursuit.
    /// This will target the player vehicle instead of an actual suspect.
    /// </summary>
    public class DispatchSpawnComponent : IMenuComponent<UIMenuListItem>
    {
        private readonly IPursuitManager _pursuitManager;

        public DispatchSpawnComponent(IPursuitManager pursuitManager)
        {
            _pursuitManager = pursuitManager;
        }

        /// <inheritdoc />
        public UIMenuListItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.DispatchSpawn, AutomaticRoadblocksPlugin.DispatchSpawnDescription,
            new List<IDisplayItem>
            {
                new DisplayItem(DispatchSpawnType.Calculate, AutomaticRoadblocksPlugin.DispatchPreviewCalculateType),
                new DisplayItem(DispatchSpawnType.CurrentLocation, AutomaticRoadblocksPlugin.DispatchPreviewCurrentLocationType)
            });

        /// <inheritdoc />
        public MenuType Type => MenuType.PURSUIT;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            var selectedValue = MenuItem.SelectedItem.Value;
            if (selectedValue.GetType() != typeof(DispatchSpawnType))
                return;

            _pursuitManager.DispatchNow(false, true, (DispatchSpawnType)selectedValue == DispatchSpawnType.CurrentLocation);
        }

        private enum DispatchSpawnType
        {
            Calculate,
            CurrentLocation
        }
    }
}