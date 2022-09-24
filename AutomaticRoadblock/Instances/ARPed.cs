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
        public Ped GameInstance { get; }

        /// <summary>
        /// Get the primary weapon of this ped.
        /// </summary>
        public WeaponDescriptor PrimaryWeapon => GameInstance.Inventory.Weapons.First(x => x.MagazineSize > 0);

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
            EntityUtils.Remove(GameInstance);
        }

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
            ClearAllTasks();
            GameInstance.KeepTasks = false;
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
        public void AimAt(Entity entity, int duration)
        {
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

        public void WarpIntoVehicle(Vehicle vehicle, EVehicleSeat seat)
        {
            Assert.NotNull(vehicle, "vehicle cannot be null");
            if (IsInvalid || !vehicle.IsValid())
                return;

            GameInstance?.WarpIntoVehicle(vehicle, (int)seat);
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

            GameInstance.IsPersistent = true;
            GameInstance.KeepTasks = true;
            GameInstance.RelationshipGroup = RelationshipGroup.Cop;
            Functions.SetPedAsCop(GameInstance);
            Functions.SetCopAsBusy(GameInstance, true);
        }

        #endregion
    }
}