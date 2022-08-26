using System.Linq;
using AutomaticRoadblocks.Utils.Type;
using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Utils
{
    public static class EntityUtils
    {
        /// <summary>
        /// Create a vehicle which is placed on the ground correctly.
        /// </summary>
        /// <param name="model">The model of the vehicle.</param>
        /// <param name="position">The position of the vehicle.</param>
        /// <param name="heading">The heading of the vehicle.</param>
        /// <returns>Returns the created vehicle.</returns>
        public static Vehicle CreateVehicle(Model model, Vector3 position, float heading)
        {
            Assert.NotNull(model, "model cannot be null");
            Assert.NotNull(position, "position cannot be null");
            return PutVehicleOnTheGround(new Vehicle(model, position, heading));
        }

        /// <summary>
        /// Put the vehicle correctly on the ground.
        /// This prevents a vehicle from jumping up when it is being spawned.
        /// </summary>
        /// <param name="vehicle">The vehicle to put on the ground.</param>
        public static Vehicle PutVehicleOnTheGround(Vehicle vehicle)
        {
            Assert.NotNull(vehicle, "vehicle cannot be null");
            NativeFunction.Natives.SET_VEHICLE_ON_GROUND_PROPERLY(vehicle);
            return vehicle;
        }

        /// <summary>
        /// Attach the given attachment to the given entity.
        /// </summary>
        /// <param name="attachment">Set the attachment entity.</param>
        /// <param name="target">Set the target entity.</param>
        /// <param name="placement">Set the place to attach the attachment to at the target.</param>
        public static void AttachEntity(Entity attachment, Entity target, PedBoneId placement)
        {
            Assert.NotNull(attachment, "attachment cannot be null");
            Assert.NotNull(target, "target cannot be null");
            var boneId = NativeFunction.Natives.GET_PED_BONE_INDEX<int>(target, (int)placement);

            NativeFunction.Natives.ATTACH_ENTITY_TO_ENTITY(attachment, target, boneId, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, false, false, false, 2, 1);
        }

        /// <summary>
        /// Detach the given attachment from it's entity.
        /// </summary>
        /// <param name="attachment">Set the attachment.</param>
        public static void DetachEntity(Entity attachment)
        {
            Assert.NotNull(attachment, "attachment cannot be null");
            NativeFunction.Natives.DETACH_ENTITY(attachment, false, false);
        }

        /// <summary>
        /// Set if the given ped entity can cower while in cover.
        /// </summary>
        /// <param name="ped">The ped to set the cower state.</param>
        /// <param name="canCower">Set the cower state.</param>
        public static void CanCowerInCover(Ped ped, bool canCower)
        {
            Assert.NotNull(ped, "ped cannot be null");
            NativeFunction.Natives.SET_PED_CAN_COWER_IN_COVER(ped, canCower);
        }

        /// <summary>
        /// Set if the given ped entity can peek while in cover.
        /// </summary>
        /// <param name="ped">The ped to set the cower state.</param>
        /// <param name="canPeek">Set the cower state.</param>
        public static void CanPeekInCover(Ped ped, bool canPeek)
        {
            Assert.NotNull(ped, "ped cannot be null");
            NativeFunction.Natives.SET_PED_CAN_PEEK_IN_COVER(ped, canPeek);
        }

        /// <summary>
        /// Load the cover for the given ped.
        /// </summary>
        /// <param name="ped">The ped to set the cower state.</param>
        /// <param name="loadCoverState">Set if the cover data should be loaded for the ped.</param>
        public static void LoadCover(Ped ped, bool loadCoverState)
        {
            Assert.NotNull(ped, "ped cannot be null");
            NativeFunction.Natives.SET_PED_TO_LOAD_COVER(ped, loadCoverState);
        }

        /// <summary>
        /// Remove the given entity from the game world.
        /// </summary>
        /// <param name="entity">Set the entity to remove.</param>
        public static void Remove(Entity entity)
        {
            Assert.NotNull(entity, "entity cannot be null");
            if (!entity.IsValid())
                return;

            entity.IsPersistent = false;
            entity.Dismiss();
            entity.Delete();
        }

        /// <summary>
        /// Clean the given area from entities (e.g. vehicles, objects, etc.)
        /// </summary>
        /// <param name="position">Set the position to clean.</param>
        /// <param name="radius">Set the area around the position to clean.</param>
        /// <param name="excludeEmergencyVehicles">Set if emergency vehicles should be excluded from the cleanup.</param>
        public static void CleanArea(Vector3 position, float radius, bool excludeEmergencyVehicles = false)
        {
            Assert.NotNull(position, "position cannot be null");
            var queryFlags = GetEntitiesFlags.ConsiderAllVehicles | GetEntitiesFlags.ExcludePlayerPed | GetEntitiesFlags.ExcludePlayerVehicle;

            if (excludeEmergencyVehicles)
                queryFlags |= GetEntitiesFlags.ExcludeAmbulances | GetEntitiesFlags.ExcludeFiretrucks | GetEntitiesFlags.ExcludePoliceCars;

            var entitiesToClean = World
                .GetEntities(position, radius, queryFlags)
                .Where(x => x.IsValid())
                .Where(x => x != Game.LocalPlayer.LastVehicle);

            foreach (var entity in entitiesToClean)
            {
                Remove(entity);
            }
        }

        /// <summary>
        /// Set the vehicle lights state.
        /// </summary>
        /// <param name="vehicle">The vehicle to modify.</param>
        /// <param name="state">The state of the vehicle lights.</param>
        public static void VehicleLights(Vehicle vehicle, VehicleLightState state)
        {
            Assert.NotNull(vehicle, "vehicle cannot be null");
            NativeFunction.Natives.SET_VEHICLE_LIGHTS(vehicle, (int)state);
        }
    }
}