using System;
using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Roadblock;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Pursuit.Menu
{
    public class PursuitDispatchPreviewComponent : IMenuComponent<UIMenuListScrollerItem<ERoadblockDistance>>
    {
        private readonly IPursuitManager _pursuitManager;
        private readonly IGame _game;

        public PursuitDispatchPreviewComponent(IPursuitManager pursuitManager, IGame game)
        {
            _pursuitManager = pursuitManager;
            _game = game;
        }

        /// <inheritdoc />
        public UIMenuListScrollerItem<ERoadblockDistance> MenuItem { get; } = new(AutomaticRoadblocksPlugin.DispatchPreview,
            AutomaticRoadblocksPlugin.DispatchPreviewDescription, new[]
            {
                ERoadblockDistance.CurrentLocation,
                ERoadblockDistance.Closely,
                ERoadblockDistance.Default,
                ERoadblockDistance.Far,
                ERoadblockDistance.VeryFar,
                ERoadblockDistance.ExtremelyFar,
            });

        /// <inheritdoc />
        public EMenuType Type => EMenuType.Pursuit;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _game.NewSafeFiber(() => _pursuitManager.DispatchPreview(MenuItem.SelectedItem), "PursuitDispatchPreviewComponent.OnMenuActivation");
        }

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            MenuItem.Formatter = distance => distance switch
            {
                ERoadblockDistance.CurrentLocation => AutomaticRoadblocksPlugin.DispatchPreviewCurrentLocation,
                ERoadblockDistance.Closely => AutomaticRoadblocksPlugin.DispatchPreviewClosely,
                ERoadblockDistance.Default => AutomaticRoadblocksPlugin.DispatchPreviewDefault,
                ERoadblockDistance.Far => AutomaticRoadblocksPlugin.DispatchPreviewFar,
                ERoadblockDistance.VeryFar => AutomaticRoadblocksPlugin.DispatchPreviewVeryFar,
                ERoadblockDistance.ExtremelyFar => AutomaticRoadblocksPlugin.DispatchPreviewExtremelyFar,
                _ => throw new ArgumentOutOfRangeException(nameof(distance), distance, "unsupported menu option")
            };
        }
    }
}