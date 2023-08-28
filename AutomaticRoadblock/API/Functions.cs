using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.CloseRoad;
using AutomaticRoadblocks.Logging;
using AutomaticRoadblocks.ManualPlacement;
using AutomaticRoadblocks.Models;
using AutomaticRoadblocks.Pursuit;
using AutomaticRoadblocks.RedirectTraffic;
using AutomaticRoadblocks.Roadblock.Dispatcher;
using AutomaticRoadblocks.SpikeStrip.Dispatcher;
using Rage;

namespace AutomaticRoadblocks.API
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class Functions
    {
        /// <summary>
        /// Get the roadblock dispatcher.
        /// This dispatcher allows deploying of automatic pursuit roadblocks.
        /// </summary>
        public static IRoadblockDispatcher RoadblockDispatcher => IoC.Instance.GetInstance<IRoadblockDispatcher>();

        /// <summary>
        /// Get the automatic roadblock pursuit manager.
        /// This manager manages the settings of the roadblocks during a pursuit.
        /// </summary>
        public static IPursuitManager PursuitManager => IoC.Instance.GetInstance<IPursuitManager>();

        /// <summary>
        /// Get the manual placement instance.
        /// This instance allows placing manual roadblocks.
        /// </summary>
        public static IManualPlacement ManualPlacement => IoC.Instance.GetInstance<IManualPlacement>();

        /// <summary>
        /// Get the traffic redirection dispatcher.
        /// This dispatcher allows deploying traffic redirections.
        /// </summary>
        public static IRedirectTrafficDispatcher RedirectTrafficDispatcher => IoC.Instance.GetInstance<IRedirectTrafficDispatcher>();

        /// <summary>
        /// Get the spike strip dispatcher instance.
        /// </summary>
        public static ISpikeStripDispatcher SpikeStripDispatcher => IoC.Instance.GetInstance<ISpikeStripDispatcher>();

        /// <summary>
        /// Get the close road dispatcher instance.
        /// </summary>
        public static ICloseRoadDispatcher CloseRoadDispatcher => IoC.Instance.GetInstance<ICloseRoadDispatcher>();
        
        /// <summary>
        /// The model provider for barriers and light sources.
        /// This provider uses the automatic roadblock data files to determine the available models.
        /// </summary>
        public static IModelProvider ModelProvider => IoC.Instance.GetInstance<IModelProvider>();

        /// <summary>
        /// Dispatch a roadblock for the current pursuit.
        /// This method has been introduced for Grammar Police and is the same as using <code>PursuitManager.DispatchNow(xxx)</code>.
        /// </summary>
        /// <param name="userRequested">Indicates if the roadblock was called by the user.</param>
        /// <returns>Returns true if a roadblock is dispatched, else false.</returns>
        public static bool DispatchPursuitRoadblockNow(bool userRequested = false)
        {
            var logger = IoC.Instance.GetInstance<ILogger>();
            logger.Info($"Dispatching roadblock from functions API with {nameof(userRequested)}: {userRequested}");
            return PursuitManager.DispatchNow(userRequested);
        }

        /// <summary>
        /// Close the current road at the current player location.
        /// </summary>
        public static void CloseRoad()
        {
            CloseRoadDispatcher.CloseNearbyRoad(Game.LocalPlayer.Character.Position);
        }
    }
}