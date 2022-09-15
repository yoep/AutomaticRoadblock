using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Models;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public abstract class AbstractBarrierComponentItem : IMenuComponent<UIMenuListScrollerItem<BarrierModel>>
    {
        protected readonly IManualPlacement ManualPlacement;
        private readonly IModelProvider _modelProvider;
        private readonly ILocalizer _localizer;

        public AbstractBarrierComponentItem(IManualPlacement manualPlacement, IModelProvider modelProvider, ILocalizer localizer, LocalizationKey name,
            LocalizationKey description)
        {
            ManualPlacement = manualPlacement;
            _modelProvider = modelProvider;
            _localizer = localizer;

            MenuItem = new UIMenuListScrollerItem<BarrierModel>(localizer[name], localizer[description], FilterItems(_modelProvider.BarrierModels));
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<BarrierModel> MenuItem { get; }

        /// <inheritdoc />
        public EMenuType Type => EMenuType.ManualPlacement;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            // no-op   
        }

        /// <summary>
        /// The action to invoke when the index of the list is changed.
        /// </summary>
        protected abstract void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex);

        protected abstract void SelectInitialMenuItem();

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            MenuItem.Formatter = model => _localizer[model.LocalizationKey];
            SelectInitialMenuItem();
            MenuItem.IndexChanged += MenuIndexChanged;
            _modelProvider.BarrierModelsChanged += BarrierModelsChanged;
        }

        private void BarrierModelsChanged(IEnumerable<BarrierModel> models)
        {
            MenuItem.Items = FilterItems(models);
        }

        private static IList<BarrierModel> FilterItems(IEnumerable<BarrierModel> models)
        {
            return models == null
                ? new List<BarrierModel> { BarrierModel.None }
                : models.Where(x => x.Barrier.Flags.HasFlag(EBarrierFlags.ManualPlacement)).ToList();
        }
    }
}