using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public abstract class AbstractRoadblockSlot : IRoadblockSlot
    {
        protected readonly List<InstanceSlot> Instances = new List<InstanceSlot>();

        protected AbstractRoadblockSlot(Vector3 position, float heading)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(heading, "heading cannot be null");
            Position = position;
            Heading = heading;
        }

        #region Properties

        /// <inheritdoc />
        public Vector3 Position { get; }

        /// <inheritdoc />
        public float Heading { get; }

        /// <inheritdoc />
        public Vehicle Vehicle => Instances
            .Where(x => x.Type == EntityType.CopVehicle)
            .Select(x => x.Instance)
            .Select(x => (Vehicle)x)
            .FirstOrDefault();

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => Instances.First().IsPreviewActive;


        /// <inheritdoc />
        public void CreatePreview()
        {
            Instances.ForEach(x => x.CreatePreview());
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            Instances.ForEach(x => x.DeletePreview());
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Instances.ForEach(x => x.Dispose());
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Instances)}: {Instances.Count},\n" +
                   $"{nameof(Position)}: {Position},\n" +
                   $"{nameof(Heading)}: {Heading}";
        }

        public void Spawn()
        {
            if (IsPreviewActive)
                DeletePreview();

            Instances.ForEach(x => x.Spawn());
            Instances
                .Where(x => x.Type == EntityType.CopPed)
                .Select(x => x.Instance)
                .Select(x => (Ped)x)
                .ToList()
                .ForEach(x =>
                {
                    Functions.SetPedAsCop(x);
                    Functions.SetCopAsBusy(x, true);
                    x.WarpIntoVehicle(Vehicle, (int)VehicleSeat.Any);
                });
        }

        public void ReleaseToLspdfr()
        {
            Instances
                .Where(x => x.Type == EntityType.CopPed)
                .ToList()
                .ForEach(x => Functions.SetCopAsBusy((Ped)x.Instance, false));
        }
    }
}