using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Models;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.RedirectTraffic.Menu
{
    public class RedirectTrafficConeTypeComponentItem : IMenuComponent<UIMenuListScrollerItem<BarrierModel>>
    {
        private readonly IRedirectTrafficDispatcher _redirectTrafficDispatcher;
        private readonly IModelProvider _modelProvider;
        private readonly ILocalizer _localizer;

        public RedirectTrafficConeTypeComponentItem(IRedirectTrafficDispatcher redirectTrafficDispatcher, IModelProvider modelProvider, ILocalizer localizer)
        {
            _redirectTrafficDispatcher = redirectTrafficDispatcher;
            _modelProvider = modelProvider;
            _localizer = localizer;

            MenuItem = new UIMenuListScrollerItem<BarrierModel>(localizer[LocalizationKey.Barrier], localizer[LocalizationKey.BarrierDescription],
                FilterItems(_modelProvider.BarrierModels));
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<BarrierModel> MenuItem { get; }

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
            MenuItem.Formatter = model => _localizer[model.LocalizationKey];
            MenuItem.SelectedItem = _redirectTrafficDispatcher.ConeType;
            MenuItem.IndexChanged += MenuIndexChanged;
            _modelProvider.BarrierModelsChanged += BarrierModelsChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _redirectTrafficDispatcher.ConeType = MenuItem.SelectedItem;
        }

        private void BarrierModelsChanged(IEnumerable<BarrierModel> models)
        {
            MenuItem.Items = FilterItems(models);
        }

        private static IList<BarrierModel> FilterItems(IEnumerable<BarrierModel> models)
        {
            return models == null
                ? new List<BarrierModel> { BarrierModel.None }
                : models.Where(x => x.Barrier.Flags.HasFlag(EBarrierFlags.RedirectTraffic)).ToList();
        }
    }
}