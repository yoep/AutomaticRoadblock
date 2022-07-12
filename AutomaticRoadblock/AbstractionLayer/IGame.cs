using System;
using Rage;

namespace AutomaticRoadblocks.AbstractionLayer
{
    public interface IGame
    {
        /// <summary>
        /// Get the game time in milliseconds.
        /// Unlike <see cref="P:System.Environment.TickCount" /> this value is scaled based on the game time dilation.
        /// </summary>
        uint GameTime { get; }

        /// <summary>
        /// Get the player's current position.
        /// </summary>
        Vector3 PlayerPosition { get; }
        
        /// <summary>
        /// Get the heading of the player character.
        /// </summary>
        float PlayerHeading { get; }

        /// <summary>
        /// Get the player's last vehicle.
        /// This property will be null when the player doesn't have a last vehicle.
        /// </summary>
        Vehicle PlayerVehicle { get; }

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

        /// <summary>
        /// Display a notification with the name of the plugin at the start.
        /// </summary>
        /// <param name="message">Set the message to show in the notification.</param>
        void DisplayPluginNotification(string message);

        /// <summary>
        /// Display a notification.
        /// </summary>
        /// <param name="message">Set the message to display in a notification.</param>
        void DisplayNotification(string message);
    }
}