using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils.Road;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public abstract class AbstractPursuitRoadblockSlot : AbstractRoadblockSlot
    {
        private bool _monitoring;

        protected AbstractPursuitRoadblockSlot(Road.Lane lane, BarrierType barrierType, float heading, Vehicle targetVehicle, bool shouldAddLights)
            : base(lane, barrierType, heading, shouldAddLights)
        {
            Assert.NotNull(targetVehicle, "targetVehicle cannot be null");
            TargetVehicle = targetVehicle;
        }

        #region Properties

        /// <summary>
        /// Get the target vehicle of the slot.
        /// </summary>
        protected Vehicle TargetVehicle { get; }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public override void Dispose()
        {
            _monitoring = false;
            base.Dispose();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Spawn()
        {
            base.Spawn();
            Monitor();
        }

        #endregion

        #region Functions

        private void Monitor()
        {
            var game = IoC.Instance.GetInstance<IGame>();
            _monitoring = true;

            game.NewSafeFiber(() =>
            {
                while (_monitoring)
                {
                    var hasACopBeenKilled = Instances
                        .Where(x => x.Type == EntityType.CopPed)
                        .Select(x => (ARPed)x.Instance)
                        .Where(x => x.GameInstance.IsDead)
                        .Where(x => x.GameInstance.HasBeenDamagedByAnyVehicle)
                        .Any(x => x.GameInstance.HasBeenDamagedBy(TargetVehicle));

                    if (hasACopBeenKilled)
                        InvokedCopHasBeenKilled();

                    game.FiberYield();
                }
            }, "AbstractPursuitRoadblockSlot.Monitor");
        }

        #endregion
    }
}