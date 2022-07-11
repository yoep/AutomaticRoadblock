using System;
using System.Diagnostics.CodeAnalysis;
using Rage;

namespace AutomaticRoadblocks.Instance
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface ARInstance<out TType> : IDisposable where TType : Entity
    {
        /// <summary>
        /// Get the game instance of this ARInstance.
        /// </summary>
        TType GameInstance { get; }
    }
}