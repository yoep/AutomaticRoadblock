using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.CloseRoad.Menu
{
    public class CloseRoadUnitTypeComponentItem  : IMenuComponent<UIMenuListScrollerItem<EBackupUnit>>
    {
        private readonly ICloseRoadDispatcher _closeRoadDispatcher;
        private readonly ILocalizer _localizer;

        public CloseRoadUnitTypeComponentItem(ICloseRoadDispatcher closeRoadDispatcher, ILocalizer localizer)
        {
            _closeRoadDispatcher = closeRoadDispatcher;
            _localizer = localizer;
            
            MenuItem = new UIMenuListScrollerItem<EBackupUnit>(localizer[LocalizationKey.Vehicle], localizer[LocalizationKey.VehicleDescription],
                LspdfrHelper.BackupUnits());
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<EBackupUnit> MenuItem { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.CloseRoad;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            // no-op   
        }

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            MenuItem.Formatter = type => _localizer[LspdfrHelper.ToLocalizationKey(type)];
            MenuItem.SelectedItem = _closeRoadDispatcher.BackupUnit;
            MenuItem.IndexChanged += MenuIndexChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _closeRoadDispatcher.BackupUnit = MenuItem.SelectedItem;
        }
    }
}