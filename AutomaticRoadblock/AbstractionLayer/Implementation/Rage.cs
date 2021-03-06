using System;
using System.Threading;
using Rage;

namespace AutomaticRoadblocks.AbstractionLayer.Implementation
{
    public class Rage : IGame
    {
        private readonly ILogger _logger;

        public Rage(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public uint GameTime => Game.GameTime;

        /// <inheritdoc />
        public Vector3 PlayerPosition => Game.LocalPlayer.Character.Position;

        /// <inheritdoc />
        public float PlayerHeading => Game.LocalPlayer.Character.Heading;

        /// <inheritdoc />
        public Vehicle PlayerVehicle => Game.LocalPlayer.LastVehicle;

        /// <inheritdoc />
        public void NewSafeFiber(Action action, string name)
        {
            GameFiber.StartNew(() =>
            {
                try
                {
                    action.Invoke();
                }
                catch (ThreadInterruptedException)
                {
                    //ignore as this is probably on plugin termination and thread is in waiting state
                }
                catch (ThreadAbortException)
                {
                    //ignore as this is probably on plugin termination and thread was executing a method and couldn't exit correctly
                }
                catch (Exception ex)
                {
                    _logger.Error("An unexpected error occurred in '" + name + "' thread, error: " + ex.Message, ex);
                    DisplayPluginNotification("~r~" + name + " thread has stopped working, see logs for more info");
                }
            }, name);
        }

        /// <inheritdoc />
        public void FiberYield()
        {
            GameFiber.Yield();
        }

        /// <inheritdoc />
        public void DisplayPluginNotification(string message)
        {
            Game.DisplayNotification("~b~" + AutomaticRoadblocksPlugin.Name + " ~s~" + message.Trim());
        }

        /// <inheritdoc />
        public void DisplayNotification(string message)
        {
            Game.DisplayNotification(message.Trim());
        }
    }
}