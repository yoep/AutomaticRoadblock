using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Models;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.ManualPlacement.Menu
{
    public class ManualPlacementLightTypeComponentItem : IMenuComponent<UIMenuListScrollerItem<LightModel>>
    {
        private readonly IManualPlacement _manualPlacement;
        private readonly ILocalizer _localizer;
        private readonly IModelProvider _modelProvider;

        public ManualPlacementLightTypeComponentItem(IManualPlacement manualPlacement, ILocalizer localizer, IModelProvider modelProvider)
        {
            _manualPlacement = manualPlacement;
            _localizer = localizer;
            _modelProvider = modelProvider;

            MenuItem = new UIMenuListScrollerItem<LightModel>(localizer[LocalizationKey.LightSource], localizer[LocalizationKey.LightSourceDescription],
                modelProvider.LightModels);
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<LightModel> MenuItem { get; }

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
            MenuItem.Formatter = model => _localizer[model.LocalizationKey];
            MenuItem.Items = new List<LightModel> { LightModel.None };
            MenuItem.SelectedItem = _manualPlacement.LightSourceType;
            MenuItem.IndexChanged += MenuIndexChanged;
            _modelProvider.LightModelsChanged += LightModelsChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _manualPlacement.LightSourceType = MenuItem.SelectedItem;
        }

        private void LightModelsChanged(IEnumerable<LightModel> models)
        {
            MenuItem.Items = models.ToList();
        }
    }
}