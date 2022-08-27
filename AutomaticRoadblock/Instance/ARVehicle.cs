using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Type;
using Rage;

namespace AutomaticRoadblocks.Instance
{
    /// <summary>
    /// A vehicle which is controlled by the Automatic Roadblock plugin.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ARVehicle : IARInstance<Vehicle>
    {
        public ARVehicle(Model model, Vector3 position, float heading = 0f, bool recordCollisions = false)
        {
            Assert.NotNull(model, "model cannot be null");
            Assert.NotNull(position, "position cannot be null");
            GameInstance = EntityUtils.CreateVehicle(model, position, heading);
            GameInstance.IsPersistent = true;
            GameInstance.NeedsCollision = true;
            GameInstance.IsRecordingCollisions = recordCollisions;
            GameInstance.IsEngineOn = true;
            GameInstance.IsSirenOn = true;
            EntityUtils.VehicleLights(GameInstance, VehicleLightState.AlwaysOn);
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

        #region IARInstance

        /// <inheritdoc />
        public void Release()
        {
            if (GameInstance == null || !GameInstance.IsValid())
                return;

            GameInstance.IsPersistent = false;
            GameInstance.IsRecordingCollisions = false;
        }

        #endregion
    }
}