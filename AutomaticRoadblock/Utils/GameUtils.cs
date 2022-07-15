using System.Drawing;
using AutomaticRoadblocks.Utils.Type;
using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Utils
{
    public static class GameUtils
    {
        /// <summary>
        /// Get the current hour of the game.
        /// </summary>
        /// <returns>Returns the hour of the game.</returns>
        public static int GetCurrentHour()
        {
            return World.TimeOfDay.Hours;
        }

        /// <summary>
        /// Get the current time period of the game.
        /// </summary>
        public static TimePeriod TimePeriod
        {
            get
            {
                var hour = GetCurrentHour();

                return hour switch
                {
                    >= 6 and < 8 => TimePeriod.Morning,
                    >= 8 and < 12 => TimePeriod.Forenoon,
                    >= 12 and < 18 => TimePeriod.Afternoon,
                    >= 18 and < 20 => TimePeriod.Evening,
                    _ => TimePeriod.Night
                };

                // otherwise, it's night
            }
        }

        /// <summary>
        /// Get a new vector that is placed on the ground in the game world for the current position.
        /// </summary>
        /// <param name="position">The game world position.</param>
        /// <returns>Returns a new Vector that is placed on the ground.</returns>
        public static Vector3 GetOnTheGroundVector(Vector3 position)
        {
            var newPosition = new Vector3(position.X, position.Y, 0f);

            NativeFunction.Natives.GET_GROUND_Z_FOR_3D_COORD<bool>(position.X, position.Y, position.Z + 15f, out float newHeight, 0);
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
        /// <param name="rotate">Set if the marker should be rotating or not.</param>
        public static void CreateMarker(Vector3 position, MarkerType markerType, Color color, float size = 1f, bool rotate = true)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(markerType, "markerType cannot be null");
            Assert.NotNull(color, "color cannot be null");
            NativeFunction.CallByName<uint>("DRAW_MARKER", (int)markerType, position.X, position.Y, position.Z, position.X, position.Y, position.Z, 0f, 0f, 0f,
                size, size, size, color.R, color.G, color.B, 100, false, rotate, "", "", false);
        }
    }
}