namespace AutomaticRoadblock.AbstractionLayer
{
    /// <summary>
    /// Abstraction interface for displaying notifications to the player.
    /// </summary>
    public interface INotification
    {
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