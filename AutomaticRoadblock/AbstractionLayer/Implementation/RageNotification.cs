using Rage;

namespace AutomaticRoadblocks.AbstractionLayer.Implementation
{
    public class RageNotification : INotification
    {
        /// <inheritdoc />
        public void DisplayPluginNotification(string message)
        {
            Game.DisplayNotification("~b~" + AutomaticRoadblockPlugin.Name + " ~s~" + message.Trim());
        }

        /// <inheritdoc />
        public void DisplayNotification(string message)
        {
            Game.DisplayNotification(message.Trim());
        }
    }
}