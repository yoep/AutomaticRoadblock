using System.Collections.Generic;
using AutomaticRoadblocks.Menu;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Pursuit.Menu
{
    public class DispatchPreviewComponent : IMenuComponent<UIMenuListItem>
    {
        private readonly IPursuitManager _pursuitManager;

        public DispatchPreviewComponent(IPursuitManager pursuitManager)
        {
            _pursuitManager = pursuitManager;
        }

        /// <inheritdoc />
        public UIMenuListItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.DispatchPreview,
            AutomaticRoadblocksPlugin.DispatchPreviewDescription, new List<IDisplayItem>
            {
                new DisplayItem(DispatchPreviewType.Calculate, AutomaticRoadblocksPlugin.DispatchPreviewCalculateType),
                new DisplayItem(DispatchPreviewType.CurrentLocation, AutomaticRoadblocksPlugin.DispatchPreviewCurrentLocationType)
            });

        /// <inheritdoc />
        public MenuType Type => MenuType.Pursuit;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            var selectedValue = MenuItem.SelectedItem.Value;
            if (selectedValue.GetType() != typeof(DispatchPreviewType))
                return;

            _pursuitManager.DispatchPreview((DispatchPreviewType)selectedValue == DispatchPreviewType.CurrentLocation);
        }

        private enum DispatchPreviewType
        {
            Calculate,
            CurrentLocation
        }
    }
}