using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutomaticRoadblocks.Utils;
using AutomaticRoadblocks.Utils.Type;
using JetBrains.Annotations;
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
        private readonly List<Entity> _attachments = new();

        public ARPed(Model model, Vector3 position, float heading = 0f)
        {
            Assert.NotNull(model, "model cannot be null");
            Assert.NotNull(position, "position cannot be null");
            GameInstance = new Ped(model, position, heading)
            {
                IsPersistent = true,
                KeepTasks = true,
                RelationshipGroup = RelationshipGroup.Cop
            };
            RegisterCopToLspdfr();
        }

        #region Properties

        /// <inheritdoc />
        [CanBeNull]
        public Ped GameInstance { get; }

        /// <summary>
        /// Get the primary weapon of this ped.
        /// </summary>
        public WeaponDescriptor PrimaryWeapon { get; private set; }

        /// <inheritdoc />
        public bool IsInvalid => GameInstance == null ||
                                 !GameInstance.IsValid();

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
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
            GameInstance.KeepTasks = false;
            GameInstance.IsPersistent = false;
            Functions.SetPedAsCop(GameInstance);
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
        /// Give the ped a primary weapon which is added in the inventory.
        /// For the list of weapon names, see <see cref="ModelUtils.Weapons"/>.
        /// </summary>
        /// <param name="name">The weapon name to give.</param>
        /// <param name="equipNow">Set if the weapon should be instantly equipped.</param>
        public void GivePrimaryWeapon(string name, bool equipNow = true)
        {
            PrimaryWeapon = CreateWeaponInInventory(name, equipNow);
        }

        /// <summary>
        /// Give the ped the given weapon name in the inventory.
        /// </summary>
        /// <param name="name"></param>
        public void GiveWeapon(string name)
        {
            CreateWeaponInInventory(name);
        }

        /// <summary>
        /// Equip the primary weapon if one is set through <see cref="GivePrimaryWeapon"/>.
        /// </summary>
        public void EquipPrimaryWeapon()
        {
            if (IsInvalid)
                return;

            GameInstance.Inventory.EquippedWeapon = PrimaryWeapon;
        }

        /// <summary>
        /// Let the ped cover behind the closest object.
        /// </summary>
        public void Cover()
        {
            EntityUtils.CanPeekInCover(GameInstance, true);
            EntityUtils.LoadCover(GameInstance, true);
        }

        /// <summary>
        /// Aim at the given entity.
        /// </summary>
        /// <param name="entity">The entity to aim at.</param>
        /// <param name="duration">The duration.</param>
        public void AimAt(Entity entity, int duration)
        {
            EquipPrimaryWeapon();
            GameInstance.Tasks.AimWeaponAt(entity, duration);
        }

        /// <summary>
        /// Fire at the given entity.
        /// </summary>
        /// <param name="entity">The entity to fire at.</param>
        /// <param name="duration">The duration.</param>
        public void FireAt(Entity entity, int duration)
        {
            Assert.NotNull(entity, "entity cannot be null");
            EquipPrimaryWeapon();
            GameInstance.Tasks.FireWeaponAt(entity, duration, FiringPattern.BurstFire);
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

        public void WarpIntoVehicle(Vehicle vehicle, VehicleSeat seat)
        {
            Assert.NotNull(vehicle, "vehicle cannot be null");
            if (IsInvalid || !vehicle.IsValid())
                return;

            GameInstance?.WarpIntoVehicle(vehicle, (int)seat);
        }

        #endregion

        #region Functions

        private void RegisterCopToLspdfr()
        {
            // Functions.SetPedAsCop(GameInstance);
            // Functions.SetCopAsBusy(GameInstance, true);
        }

        private WeaponDescriptor CreateWeaponInInventory(string name, bool equipNow = false)
        {
            return GameInstance.Inventory.GiveNewWeapon(new WeaponAsset(name), -1, equipNow);
        }

        #endregion
    }
}