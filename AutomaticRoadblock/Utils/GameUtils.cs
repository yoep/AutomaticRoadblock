using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using AutomaticRoadblocks.Logging;
using AutomaticRoadblocks.Utils.Type;
using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Utils
{
    public static class GameUtils
    {
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        /// <summary>
        /// Get the current time period of the game.
        /// </summary>
        public static ETimePeriod TimePeriod
        {
            get
            {
                var hour = World.TimeOfDay.Hours;

                return hour switch
                {
                    >= 6 and < 8 => ETimePeriod.Morning,
                    >= 8 and < 12 => ETimePeriod.Forenoon,
                    >= 12 and < 18 => ETimePeriod.Afternoon,
                    >= 18 and < 20 => ETimePeriod.Evening,
                    _ => ETimePeriod.Night
                };

                // otherwise, it's night
            }
        }

        /// <summary>
        /// Get a new vector that is placed on the ground in the game world for the current position.
        /// The Z vector needs to be above the ground before a position can be determined, otherwise, the result will be <see cref="Vector3.Zero"/>.
        /// Modify the <see cref="heightOffset"/> if needed to prevent an inconclusive <see cref="Vector3.Zero"/> result.
        /// </summary>
        /// <param name="position">The game world position.</param>
        /// <param name="heightOffset">The height offset to apply to the Z vector before getting the coordinate.</param>
        /// <returns>Returns a new Vector that is placed on the ground.</returns>
        public static Vector3 GetOnTheGroundPosition(Vector3 position, float heightOffset = 5f)
        {
            var newPosition = new Vector3(position.X, position.Y, 0f);

            NativeFunction.Natives.GET_GROUND_Z_FOR_3D_COORD<bool>(position.X, position.Y, position.Z + heightOffset, out float newHeight, 0);
            newPosition.Z = newHeight;

            return newPosition;
        }

        /// <summary>
        /// Create a marker at the given position.
        /// </summary>
        /// <param name="position">The position of the marker.</param>
        /// <param name="markerType">The marker type.</param>
        /// <param name="color">The color of the marker.</param>
        /// <param name="size">The size of the marker.</param>
        /// <param name="height">The height of the marker.</param>
        /// <param name="rotate">Set if the marker should be rotating or not.</param>
        public static void CreateMarker(Vector3 position, EMarkerType markerType, Color color, float size = 1f, float height = 1f, bool rotate = true)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(markerType, "markerType cannot be null");
            Assert.NotNull(color, "color cannot be null");
            NativeFunction.CallByName<uint>("DRAW_MARKER", (int)markerType, position.X, position.Y, position.Z, position.X, position.Y, position.Z, 0f, 0f, 0f,
                size, size, height, color.R, color.G, color.B, 100, false, rotate, "", "", false);
        }

        /// <summary>
        /// Verify if the given key is pressed.
        /// </summary>
        /// <param name="key">The primary key.</param>
        /// <param name="modifierKey">The optional modifier key which should be pressed at the same time.</param>
        /// <returns>Returns true when the key/key-combo is pressed.</returns>
        public static bool IsKeyPressed(Keys key, Keys modifierKey = Keys.None)
        {
            Assert.NotNull(key, "key cannot be null");
            Assert.NotNull(modifierKey, "secondKey cannot be null");

            return Game.IsKeyDown(key) && (modifierKey == Keys.None || Game.IsKeyDownRightNow(modifierKey));
        }

        /// <summary>
        /// The player's current position.
        /// </summary>
        public static Vector3 PlayerPosition => Game.LocalPlayer.Character.Position;

        /// <summary>
        /// The player's current heading.
        /// </summary>
        public static float PlayerHeading => Game.LocalPlayer.Character.Heading;

        /// <summary>
        /// The player's current or last entered vehicle.
        /// </summary>
        public static Vehicle PlayerVehicle => Game.LocalPlayer.LastVehicle;

        /// <summary>
        /// Create a new safe game fiber for the given action.
        /// This fiber has access to entities and prevents the plugin from crashing in case of unexpected errors.
        /// </summary>
        /// <param name="action">The action to execute on the fiber.</param>
        /// <param name="name">The new fiber name.</param>
        public static void NewSafeFiber(Action action, string name)
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
                    Logger.Error("An unexpected error occurred in '" + name + "' thread, error: " + ex.Message, ex);
                    DisplayPluginNotification("~r~" + name + " thread has stopped working, see logs for more info");
                }
            }, name);
        }

        /// <summary>
        /// Display a notification with the plugin name.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public static void DisplayPluginNotification(string message)
        {
            Assert.HasText(message, "message cannot be empty");
            Game.DisplayNotification("~b~" + AutomaticRoadblocksPlugin.Name + " ~s~" + message.Trim());
        }

        /// <summary>
        /// Display a notification.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public static void DisplayNotification(string message)
        {
            Assert.HasText(message, "message cannot be empty");
            Game.DisplayNotification(message.Trim());
        }

        [Conditional("DEBUG")]
        public static void DisplayNotificationDebug(string message)
        {
            InternalDebugNotification(message);
        }

        [Conditional("DEBUG")]
        public static void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            Rage.Debug.DrawLineDebug(start, end, color);
        }

        [Conditional("DEBUG")]
        public static void DrawArrow(Vector3 position, Vector3 direction, Rotator rotationOffset, float scale, Color color)
        {
            Rage.Debug.DrawArrowDebug(position, direction, rotationOffset, scale, color);
        }

        [Conditional("DEBUG")]
        public static void DrawSphere(Vector3 position, float radius, Color color)
        {
            Rage.Debug.DrawSphereDebug(position, radius, color);
        }

        [Conditional("DEBUG")]
        private static void InternalDebugNotification(string message)
        {
            Game.DisplayNotification(message.Trim());
        }
    }
}