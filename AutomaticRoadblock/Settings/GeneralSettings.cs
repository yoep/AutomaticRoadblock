using System.Windows.Forms;
using AutomaticRoadblocks.AbstractionLayer;

namespace AutomaticRoadblocks.Settings
{
    public class GeneralSettings
    {
        public Keys OpenMenuKey { get; internal set; }
        
        public Keys OpenMenuModifierKey { get; internal set; }
        
        /// <summary>
        /// The log level of the plugin
        /// This will decide the log verbosity of the plugin to the log file RagePluginHook.log
        /// </summary>
        public LogLevel LogLevel { get; internal set; }
    }
}