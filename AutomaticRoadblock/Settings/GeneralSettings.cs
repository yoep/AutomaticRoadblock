using System.Windows.Forms;
using AutomaticRoadblocks.Logging;

namespace AutomaticRoadblocks.Settings
{
    public class GeneralSettings
    {
        public Keys OpenMenuKey { get; internal set; }
        
        public Keys OpenMenuModifierKey { get; internal set; }
        
        /// <summary>
        /// Clean all roadblocks, manual placements and traffic redirections
        /// This will remove all scenery items and units created by the plugin
        /// </summary>
        public Keys CleanAllKey { get; internal set; }
        
        public Keys CleanAllModifierKey { get; internal set; }
        
        /// <summary>
        /// The log level of the plugin
        /// This will decide the log verbosity of the plugin to the log file RagePluginHook.log
        /// </summary>
        public ELogLevel LogLevel { get; internal set; }
    }
}