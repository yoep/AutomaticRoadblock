using System.Diagnostics.CodeAnalysis;
using AutomaticRoadblocks.Utils;
using JetBrains.Annotations;
using Rage;

namespace AutomaticRoadblocks.Instance
{
    /// <summary>
    /// A ped which is controlled by the Automatic Roadblock plugin.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ARPed : IARInstance<Ped>
    {
        public ARPed(Model model, Vector3 position, float heading = 0f)
        {
            Assert.NotNull(model, "model cannot be null");
            Assert.NotNull(position, "position cannot be null");
            GameInstance = new Ped(model, position, heading)
            {
                IsPersistent = true,
                KeepTasks = true
            };
        }

        #region Properties

        /// <inheritdoc />
        [CanBeNull]
        public Ped GameInstance { get; }

        /// <summary>
        /// Get the primary weapon of this ped.
        /// </summary>
        public WeaponDescriptor PrimaryWeapon { get; private set; }

        #endregion

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
            GameInstance.KeepTasks = false;
        }

        #endregion

        #region Methods

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
            EquipPrimaryWeapon();
            GameInstance.Tasks.FireWeaponAt(entity, duration, FiringPattern.BurstFire);
        }
        
        #endregion

        #region Functions

        private WeaponDescriptor CreateWeaponInInventory(string name, bool equipNow = false)
        {
            return GameInstance.Inventory.GiveNewWeapon(new WeaponAsset(name), -1, equipNow);
        }

        #endregion
    }
}