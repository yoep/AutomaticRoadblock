using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public abstract class AbstractRoadblockSlot : IRoadblockSlot
    {
        protected readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        protected readonly List<InstanceSlot> Instances = new();

        protected AbstractRoadblockSlot(Vector3 position, float heading, Vehicle targetVehicle)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(heading, "heading cannot be null");
            Assert.NotNull(targetVehicle, "targetVehicle cannot be null");
            Position = position;
            Heading = heading;
            TargetVehicle = targetVehicle;
            VehicleModel = GetVehicleModel();
        }

        #region Properties

        /// <inheritdoc />
        public Vector3 Position { get; }

        /// <inheritdoc />
        public float Heading { get; }

        /// <inheritdoc />
        public Vehicle Vehicle => VehicleInstance?.GameInstance;

        /// <summary>
        /// Get the police vehicle model which is used for the slot.
        /// </summary>
        protected Model VehicleModel { get; }

        /// <summary>
        /// Get the target vehicle of the slot.
        /// </summary>
        protected Vehicle TargetVehicle { get; }

        /// <summary>
        /// Get the AR vehicle instance of this slot.
        /// </summary>
        protected ARVehicle VehicleInstance => Instances
            .Where(x => x.Type == EntityType.CopVehicle)
            .Select(x => x.Instance)
            .Select(x => (ARVehicle)x)
            .FirstOrDefault();

        /// <summary>
        /// Get the cop ped instance(s) of this slot.
        /// </summary>
        protected IEnumerable<ARPed> CopInstances
        {
            get
            {
                return Instances
                    .Where(x => x.Type == EntityType.CopPed)
                    .Select(x => x.Instance)
                    .Select(x => (ARPed)x);
            }
        }

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => Instances.First().IsPreviewActive;

        /// <inheritdoc />
        public void CreatePreview()
        {
            var game = IoC.Instance.GetInstance<IGame>();

            Instances.ForEach(x => x.CreatePreview());
            game.NewSafeFiber(() =>
            {
                var direction = MathHelper.ConvertHeadingToDirection(Heading);
                var position = Position + Vector3.WorldUp * 0.25f;

                while (IsPreviewActive)
                {
                    Rage.Debug.DrawArrow(position, direction, Rotator.Zero, 2f, Color.Yellow);
                    game.FiberYield();
                }
            }, "IRoadblockSlot.CreatePreview");
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            Instances.ForEach(x => x.DeletePreview());
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            Instances.ForEach(x => x.Dispose());
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Instances)}: {Instances.Count},\n" +
                   $"{nameof(Position)}: {Position},\n" +
                   $"{nameof(Heading)}: {Heading}";
        }

        /// <inheritdoc />
        public virtual void Spawn()
        {
            if (IsPreviewActive)
                DeletePreview();

            Instances.ForEach(x => x.Spawn());
            Vehicle.IsSirenOn = true;
            CopInstances
                .Select(x => x.GameInstance)
                .ToList()
                .ForEach(x =>
                {
                    Functions.SetPedAsCop(x);
                    Functions.SetCopAsBusy(x, true);
                });
        }

        /// <inheritdoc />
        public void ReleaseToLspdfr()
        {
            var copPeds = Instances
                .Where(x => x.Type == EntityType.CopPed)
                .ToList();

            Logger.Trace($"Releasing a total of {copPeds.Count} to LSPDFR");
            copPeds
                .Select(x => x.Instance)
                .Select(x => x.GameInstance)
                .Select(x => (Ped)x)
                .ToList()
                .ForEach(x =>
                {
                    // make sure the ped is the vehicle or at least entering it
                    if (!x.IsInVehicle(Vehicle, true))
                        x.Tasks.EnterVehicle(Vehicle, (int)VehicleSeat.Any);

                    x.Dismiss();
                    Functions.SetPedAsCop(x);
                    Functions.SetCopAsBusy(x, false);
                });
        }

        #endregion

        #region Functions

        protected void InitializeVehicleSlot()
        {
            Assert.NotNull(VehicleModel, "VehicleModel has not been initialized, unable to create vehicle slot");
            Instances.Add(new InstanceSlot(EntityType.CopVehicle, Position, Heading + 90,
                (position, heading) =>
                {
                    var vehicle = new ARVehicle(VehicleModel, GameUtils.GetOnTheGroundVector(position), heading);

                    return vehicle;
                }));
        }

        protected Vector3 GetPositionBehindVehicle()
        {
            return Position + MathHelper.ConvertHeadingToDirection(Heading) * 3f;
        }

        /// <summary>
        /// Get the vehicle model of the slot.
        /// </summary>
        /// <returns>Returns the vehicle model that should be used for this slot.</returns>
        protected abstract Model GetVehicleModel();

        #endregion
    }
}