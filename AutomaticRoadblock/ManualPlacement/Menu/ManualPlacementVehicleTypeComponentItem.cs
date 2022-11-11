using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualPlacementVehicleTypeComponentItem : IMenuComponent<UIMenuListScrollerItem<EBackupUnit>>
    {
        private readonly IManualPlacement _manualPlacement;
        private readonly ILocalizer _localizer;

        public ManualPlacementVehicleTypeComponentItem(IManualPlacement manualPlacement, ILocalizer localizer)
        {
            _manualPlacement = manualPlacement;
            _localizer = localizer;

            MenuItem = new UIMenuListScrollerItem<EBackupUnit>(localizer[LocalizationKey.Vehicle], localizer[LocalizationKey.VehicleDescription],
                LspdfrHelper.BackupUnits());
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<EBackupUnit> MenuItem { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.ManualPlacement;

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
            MenuItem.SelectedItem = _manualPlacement.BackupType;
            MenuItem.IndexChanged += MenuIndexChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _manualPlacement.BackupType = MenuItem.SelectedItem;
        }
    }
}