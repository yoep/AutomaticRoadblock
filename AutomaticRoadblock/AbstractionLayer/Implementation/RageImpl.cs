using System;
using System.Drawing;
using System.Threading;
using Rage;

namespace AutomaticRoadblocks.AbstractionLayer.Implementation
{
    public class RageImpl : IGame
    {
        private readonly ILogger _logger;

        public RageImpl(ILogger logger)
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

        public void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            Rage.Debug.DrawLineDebug(start, end, color);
        }

        public void DrawArrow(Vector3 position, Vector3 direction, Rotator rotationOffset, float scale, Color color)
        {
            Rage.Debug.DrawArrowDebug(position, direction, rotationOffset, scale, color);
        }

        public void DrawSphere(Vector3 position, float radius, Color color)
        {
            Rage.Debug.DrawSphereDebug(position, radius, color);
        }
    }
}