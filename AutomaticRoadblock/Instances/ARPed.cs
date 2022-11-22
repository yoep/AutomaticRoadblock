using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Vehicles;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AutomaticRoadblocks.Instances
{
    /// <summary>
    /// A ped which is controlled by the Automatic Roadblock plugin.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ARPed : IARInstance<Ped>
    {
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        private readonly List<Entity> _attachments = new();

        public ARPed(Ped ped, float heading = 0f)
        {
            Assert.NotNull(ped, "ped cannot be null");
            GameInstance = ped;
            GameInstance.Heading = heading;
            Initialize();
        }

        #region Properties

        /// <inheritdoc />
        public EEntityType Type => EEntityType.CopPed;

        /// <inheritdoc />
        public Ped GameInstance { get; }

        /// <inheritdoc />
        public Vector3 Position
        {
            get => GameInstance.Position;
            set => GameInstance.Position = value;
        }

        /// <inheritdoc />
        public float Heading
        {
            get => GameInstance.Heading;
            set => GameInstance.Heading = value;
        }

        /// <inheritdoc />
        public bool IsInvalid => GameInstance == null ||
                                 !GameInstance.IsValid();

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive { get; private set; }

        /// <inheritdoc />
        public void CreatePreview()
        {
            if (IsPreviewActive || IsInvalid)
                return;

            IsPreviewActive = true;
            PreviewUtils.TransformToPreview(GameInstance);
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            if (!IsPreviewActive || IsInvalid)
                return;

            IsPreviewActive = false;
            PreviewUtils.TransformToNormal(GameInstance);
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            DeletePreview();
            DeleteAttachments();
            EntityUtils.Remove(GameInstance);
        }

        #endregion

        #region IARInstance

        /// <inheritdoc />
        public void Release()
        {
            if (IsInvalid)
                return;

            DeleteAttachments();
            ClearAllTasks();
            GameInstance.IsPersistent = false;
            Functions.SetCopAsBusy(GameInstance, false);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clear all tasks of this ped.
        /// </summary>
        /// <param name="force">Set if the tasks needs to be cleared immediately.</param>
        public void ClearAllTasks(bool force = false)
        {
            if (IsInvalid)
                return;

            if (force)
            {
                GameInstance.Tasks.ClearImmediately();
            }
            else
            {
                GameInstance.Tasks.Clear();
            }
        }

        /// <summary>
        /// Aim at the given entity.
        /// </summary>
        /// <param name="entity">The entity to aim at.</param>
        /// <param name="duration">The duration.</param>
        public ARPed AimAt(Entity entity, int duration)
        {
            if (IsInvalid)
                return this;
            
            GameInstance.Tasks.AimWeaponAt(entity, duration);
            return this;
        }

        /// <summary>
        /// Fire at the given entity.
        /// </summary>
        /// <param name="entity">The entity to fire at.</param>
        /// <param name="duration">The duration.</param>
        public ARPed FireAt(Entity entity, int duration)
        {
            Assert.NotNull(entity, "entity cannot be null");
            if (IsInvalid)
                return this;
            
            GameInstance.Tasks.FireWeaponAt(entity, duration, FiringPattern.BurstFire);
            return this;
        }
        
        /// <summary>
        /// Stand still for the given amount of time.
        /// </summary>
        /// <param name="duration">The duration of the action.</param>
        public ARPed StandStill(int duration)
        {
            if (IsInvalid)
                return this;
            
            GameInstance.Tasks.StandStill(duration);
            return this;
        }

        /// <summary>
        /// Attach the given entity to this ped.
        /// </summary>
        /// <param name="attachment">Set the entity to attach.</param>
        /// <param name="placement">Set the attachment placement on the ped.</param>
        public void Attach(Entity attachment, PedBoneId placement)
        {
            Assert.NotNull(attachment, "attachment cannot be null");
            if (IsInvalid)
                return;

            _attachments.Add(attachment);

            EntityUtils.AttachEntity(attachment, GameInstance, placement);
        }

        /// <summary>
        /// Delete the attachments of this ped.
        /// </summary>
        public void DeleteAttachments()
        {
            if (IsInvalid)
                return;

            foreach (var attachment in _attachments.Where(x => x.IsValid()))
            {
                EntityUtils.DetachEntity(attachment);
                attachment.Dismiss();
                attachment.Delete();
            }

            _attachments.Clear();
        }

        /// <summary>
        /// Equip a weapon which doesn't include the stun gun.
        /// </summary>
        public ARPed EquipWeapon()
        {
            if (IsInvalid)
                return this;

            foreach (var weapon in GameInstance.Inventory.Weapons)
            {
                if (weapon.MagazineSize <= 0) 
                    continue;
                
                Logger.Trace($"Equipping weapon {weapon} for cop");
                GameInstance.Inventory.EquippedWeapon = weapon;
                break;
            }

            return this;
        }

        /// <summary>
        /// Unequip all weapons from the ped.
        /// </summary>
        public void UnequipAllWeapons()
        {
            if (IsInvalid)
                return;

            var pedInventory = GameInstance.Inventory;

            if (pedInventory != null)
            {
                var weapon = pedInventory.EquippedWeaponObject;

                if (weapon != null)
                {
                    weapon.Delete();
                }
            }
            else
            {
                Logger.Warn("Unable to unequip weapon, ped inventory is invalid");
            }
        }

        public void WarpIntoVehicle(Vehicle vehicle, EVehicleSeat seat)
        {
            Assert.NotNull(vehicle, "vehicle cannot be null");
            if (IsInvalid || !vehicle.IsValid())
                return;

            GameInstance?.WarpIntoVehicle(vehicle, (int)seat);
        }

        /// <summary>
        /// Place this ped on the ground at the given position.
        /// </summary>
        /// <param name="position">The position the ped should be placed.</param>
        public void PlaceOnGroundAt(Vector3 position)
        {
            Position = GameUtils.GetOnTheGroundPosition(position) + Vector3.WorldUp * (GameInstance.Model.Dimensions.Z / 2f);
        }

        #endregion

        #region Functions

        private void Initialize()
        {
            if (IsInvalid)
            {
                Logger.Warn($"Unable to initialize {nameof(ARPed)}, {nameof(Ped)} is invalid");
                return;
            }

            // warp cop outside the vehicle
            // this should prevent the instance from being deleted when the backup unit is none
            GameInstance.Tasks.LeaveVehicle(LeaveVehicleFlags.WarpOut);

            GameInstance.IsPersistent = true;
            GameInstance.KeepTasks = true;
            GameInstance.RelationshipGroup = RelationshipGroup.Cop;
            Functions.SetCopAsBusy(GameInstance, true);
        }

        #endregion
    }
}