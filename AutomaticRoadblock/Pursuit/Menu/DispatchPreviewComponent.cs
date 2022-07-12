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
        public UIMenuListItem MenuItem { get; } = new UIMenuListItem(AutomaticRoadblocksPlugin.DispatchPreview,
            AutomaticRoadblocksPlugin.DispatchPreviewDescription, new List<IDisplayItem>
            {
                new DisplayItem(DispatchPreviewType.Preview, AutomaticRoadblocksPlugin.DispatchPreviewPreviewType),
                new DisplayItem(DispatchPreviewType.Spawn, AutomaticRoadblocksPlugin.DispatchPreviewSpawnType)
            });

        /// <inheritdoc />
        public MenuType Type => MenuType.PURSUIT;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            var selectedValue = MenuItem.SelectedItem.Value;
            if (selectedValue.GetType() != typeof(DispatchPreviewType))
                return;

            if ((DispatchPreviewType)selectedValue == DispatchPreviewType.Preview)
            {
                _pursuitManager.DispatchPreview();
            }
            else
            {
                _pursuitManager.DispatchNow(true);
            }
        }

        private enum DispatchPreviewType
        {
            Preview,
            Spawn
        }
    }
}