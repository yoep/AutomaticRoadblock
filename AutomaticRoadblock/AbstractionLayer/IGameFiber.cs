using System;

namespace AutomaticRoadblocks.AbstractionLayer
{
    public interface IGameFiber
    {
        /// <summary>
        /// Start a new thread safe game fiber which will capture exceptions if they occur and log them in the console.
        /// </summary>
        /// <param name="action">Set the action to execute on the fiber.</param>
        /// <param name="name">Set the name of the new fiber (will also be used for logging).</param>
        void NewSafeFiber(Action action, string name);

        /// <summary>
        /// Execute GameFiber.Yield in rage
        /// </summary>
        void FiberYield();
    }
}