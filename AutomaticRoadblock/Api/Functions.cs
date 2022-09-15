using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Data;
using AutomaticRoadblocks.ManualPlacement;
using AutomaticRoadblocks.Models;
using AutomaticRoadblocks.Pursuit;
using AutomaticRoadblocks.RedirectTraffic;
using AutomaticRoadblocks.Roadblock.Dispatcher;
using AutomaticRoadblocks.SpikeStrip.Dispatcher;

namespace AutomaticRoadblocks.Api
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class Functions
    {
        /// <summary>
        /// Get the roadblock dispatcher.
        /// </summary>
        public static IRoadblockDispatcher RoadblockDispatcher => IoC.Instance.GetInstance<IRoadblockDispatcher>();

        /// <summary>
        /// Get the automatic roadblock pursuit manager. 
        /// </summary>
        public static IPursuitManager PursuitManager => IoC.Instance.GetInstance<IPursuitManager>();

        /// <summary>
        /// Get the manual placement instance.
        /// </summary>
        public static IManualPlacement ManualPlacement => IoC.Instance.GetInstance<IManualPlacement>();

        /// <summary>
        /// Get the traffic redirection dispatcher.
        /// </summary>
        public static IRedirectTrafficDispatcher RedirectTrafficDispatcher => IoC.Instance.GetInstance<IRedirectTrafficDispatcher>();
        
        /// <summary>
        /// Get the spike strip dispatcher instance.
        /// </summary>
        public static ISpikeStripDispatcher SpikeStripDispatcher => IoC.Instance.GetInstance<ISpikeStripDispatcher>();
        
        /// <summary>
        /// Get the model provider instance.
        /// This provider converts the raw data from <see cref="IDataFile"/> to actual models.
        /// </summary>
        public static IModelProvider ModelProvider => IoC.Instance.GetInstance<IModelProvider>();
    }
}