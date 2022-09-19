using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficVehicleTypeComponentType : IMenuComponent<UIMenuListScrollerItem<EBackupUnit>>
    {
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;
        private readonly ILocalizer _localizer;

        public RedirectTrafficVehicleTypeComponentType(IRedirectTrafficDispatcher redirectTrafficDispatcher, ILocalizer localizer)
        {
            _redirectTrafficDispatcher = redirectTrafficDispatcher;
            _localizer = localizer;

            MenuItem = new UIMenuListScrollerItem<EBackupUnit>(localizer[LocalizationKey.Vehicle], localizer[LocalizationKey.VehicleDescription],
                LspdfrDataHelper.BackupUnits());
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<EBackupUnit> MenuItem { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.RedirectTraffic;

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
            MenuItem.Formatter = type => _localizer[LspdfrDataHelper.ToLocalizationKey(type)];
            MenuItem.SelectedItem = _redirectTrafficDispatcher.BackupType;
            MenuItem.IndexChanged += MenuIndexChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _redirectTrafficDispatcher.BackupType = MenuItem.SelectedItem;
        }
    }
}