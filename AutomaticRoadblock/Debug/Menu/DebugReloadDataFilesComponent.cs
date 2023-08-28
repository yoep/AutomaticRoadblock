using System.Collections.Generic;
using AutomaticRoadblocks.Data;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Utils;
using RAGENativeUI.Elements;

namespace AutomaticRoadblocks.Debug.Menu
{
    public class DebugReloadDataFilesComponent : IMenuComponent<UIMenuItem>
    {
        private readonly List<IDataFile> _dataFiles;

        public DebugReloadDataFilesComponent( List<IDataFile> dataFiles)
        {
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
            GameUtils.NewSafeFiber(() =>
            {
                _dataFiles.ForEach(x => x.Reload());
                GameUtils.DisplayNotification("Data files have been reloaded");
            }, "Debug.ReloadDataFiles");
        }
    }
}