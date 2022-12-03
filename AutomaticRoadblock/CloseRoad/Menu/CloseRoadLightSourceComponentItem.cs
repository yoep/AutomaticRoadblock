using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Models;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.CloseRoad.Menu
{
    public class CloseRoadLightSourceComponentItem : IMenuComponent<UIMenuListScrollerItem<LightModel>>
    {
        private readonly ICloseRoadDispatcher _closeRoadDispatcher;
        private readonly ILocalizer _localizer;
        private readonly IModelProvider _modelProvider;

        public CloseRoadLightSourceComponentItem(ICloseRoadDispatcher closeRoadDispatcher, ILocalizer localizer, IModelProvider modelProvider)
        {
            _closeRoadDispatcher = closeRoadDispatcher;
            _localizer = localizer;
            _modelProvider = modelProvider;
            
            MenuItem = new UIMenuListScrollerItem<LightModel>(localizer[LocalizationKey.LightSource], localizer[LocalizationKey.LightSourceDescription],
                modelProvider.LightModels);
        }
        
        /// <inheritdoc />
        public UIMenuListScrollerItem<LightModel> MenuItem { get; }

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
            MenuItem.Formatter = model => _localizer[model.LocalizationKey];
            MenuItem.SelectedItem = _closeRoadDispatcher.LightSource;
            MenuItem.IndexChanged += MenuIndexChanged;
            _modelProvider.LightModelsChanged += LightModelsChanged;
        }

        private void MenuIndexChanged(UIMenuScrollerItem sender, int oldIndex, int newIndex)
        {
            _closeRoadDispatcher.LightSource = MenuItem.SelectedItem;
        }

        private void LightModelsChanged(IEnumerable<LightModel> models)
        {
            MenuItem.Items = models.ToList();
        }
    }
}