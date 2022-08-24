using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Instance
{
    /// <summary>
    /// A vehicle which is controlled by the Automatic Roadblock plugin.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ARVehicle : ARInstance<Vehicle>
    {
        public ARVehicle(Model model, Vector3 position, float heading = 0f)
        {
            Assert.NotNull(model, "model cannot be null");
            Assert.NotNull(position, "position cannot be null");
            GameInstance = EntityUtils.CreateVehicle(model, position, heading);
            GameInstance.NeedsCollision = true;
            GameInstance.IsRecordingCollisions = true;
            GameInstance.IsEngineOn = true;
        }

        /// <inheritdoc />
        public Vehicle GameInstance { get; }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            EntityUtils.Remove(GameInstance);
        }

        #endregion
    }
}