using System;
using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Preview;
using Rage;

namespace AutomaticRoadblocks.RedirectTraffic
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public interface IRedirectTrafficDispatcher : IPreviewSupport, IDisposable
    {
        /// <summary>
        /// The cone distance of the redirect traffic.
        /// </summary>
        float ConeDistance { get; set; }

        /// <summary>
        /// The backup unit type to use for redirecting the traffic.
        /// </summary>
        EBackupUnit BackupType { get; set; }
        
        /// <summary>
        /// The cones which should be used to redirect the traffic.
        /// </summary>
        BarrierModel ConeType { get; set; }
        
        /// <summary>
        /// The type of the traffic redirection which should be placed.
        /// </summary>
        RedirectTrafficType Type { get; set; }
        
        /// <summary>
        /// Enable the redirect arrow based on the side of the placement.
        /// </summary>
        bool EnableRedirectionArrow { get; set; }
        
        /// <summary>
        /// The offset of the placement in regards to the node.
        /// </summary>
        float Offset { get; set; }

        /// <summary>
        /// Dispatch a new redirect traffic instance.
        /// </summary>
        [Obsolete("Use DispatchRedirection(Vector3) instead")]
        void DispatchRedirection();
        
        /// <summary>
        /// Dispatch a traffic redirection for the current position.
        /// </summary>
        /// <param name="position">The position to create a traffic redirection.</param>
        /// <returns>Returns the created traffic redirection.</returns>
        IRedirectTraffic DispatchRedirection(Vector3 position);

        /// <summary>
        /// Remove the traffic redirects.
        /// </summary>
        /// <param name="removeType">The removal type.</param>
        void RemoveTrafficRedirects(RemoveType removeType);
    }
}