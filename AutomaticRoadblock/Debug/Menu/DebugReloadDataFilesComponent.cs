using System.Collections.Generic;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Data;
using AutomaticRoadblocks.Menu;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugReloadDataFilesComponent : IMenuComponent<UIMenuItem>
    {
        private readonly IGame _game;
        private readonly List<IDataFile> _dataFiles;

        public DebugReloadDataFilesComponent(IGame game, List<IDataFile> dataFiles)
        {
            _game = game;
            _dataFiles = dataFiles;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new(AutomaticRoadblocksPlugin.ReloadDataFiles);

        /// <inheritdoc />
        public EMenuType Type => EMenuType.Debug;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _game.NewSafeFiber(() =>
            {
                _dataFiles.ForEach(x => x.Reload());
                _game.DisplayNotification("Data files have been reloaded");
            }, "Debug.ReloadDataFiles");
        }
    }
}