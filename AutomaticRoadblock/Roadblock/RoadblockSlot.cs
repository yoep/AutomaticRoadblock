using System;
using System.Collections.Generic;
using AutomaticRoadblocks.Preview;
using AutomaticRoadblocks.Scenery;
using AutomaticRoadblocks.Utils;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AutomaticRoadblocks.Roadblock
{
    public class RoadblockSlot : IPreviewSupport, IDisposable
    {
        private readonly List<ISceneryItem> _cones = new List<ISceneryItem>();
        private readonly List<Ped> _peds = new List<Ped>();

        internal RoadblockSlot(Vector3 position, float heading)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(heading, "heading cannot be null");
            Position = position;
            Heading = heading;

            Init();
        }

        #region Properties

        /// <summary>
        /// Get the position of the roadblock slot.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Get the heading of the roadblock slot.
        /// </summary>
        public float Heading { get; }

        /// <summary>
        /// Get the vehicle of the roadblock slot.
        /// If the slot has not been spawned, it will return null.
        /// </summary>
        public Vehicle Vehicle { get; private set; }

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive { get; private set; }

        /// <inheritdoc />
        public void CreatePreview()
        {
            IsPreviewActive = true;
            CreateVehicle();
            PreviewUtils.TransformToPreview(Vehicle);
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            EntityUtils.Remove(Vehicle);
            IsPreviewActive = false;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _peds.ForEach(EntityUtils.Remove);
            EntityUtils.Remove(Vehicle);
        }

        #endregion

        public void Spawn()
        {
            if (IsPreviewActive)
                DeletePreview();

            CreateVehicle();
            CreatePeds();
        }

        public void ReleaseToLspdfr()
        {
            _peds.ForEach(x => Functions.SetCopAsBusy(x, false));
        }

        private void Init()
        {
        }

        private void CreatePeds()
        {
            var totalOccupants = ModelUtils.IsBike(Vehicle.Model) ? 1 : 2;
            
            for (var i = 0; i < totalOccupants; i++)
            {
                var ped = EntityUtils.CreateLocalCop(Position);
                var seat = i == 0 ? VehicleSeat.Driver : VehicleSeat.RightFront;

                Functions.SetPedAsCop(ped);
                Functions.SetCopAsBusy(ped, true);

                ped.WarpIntoVehicle(Vehicle, (int)seat);

                _peds.Add(ped);
            }
        }

        private void CreateVehicle()
        {
            Vehicle = new Vehicle(ModelUtils.GetLocalPolice(Position), Position, Heading + 90)
            {
                NeedsCollision = true,
                IsSirenOn = true
            };
        }
    }
}