using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public class RoadblockSlotLevel2 : AbstractRoadblockSlot
    {
        internal RoadblockSlotLevel2(Vector3 position, float heading) : base(position, heading)
        {
            Init();
        }

        private void Init()
        {
            InitializeVehicleSlot();
            InitializePedSlots();
            InitializeBarriers();
        }

        private void InitializeVehicleSlot()
        {
            Instances.Add(new InstanceSlot(EntityType.CopVehicle, Position, Heading + 90,
                (position, heading) => new Vehicle(ModelUtils.GetLocalPolice(position, false), position, heading)
                {
                    NeedsCollision = true
                }));
        }

        private void InitializePedSlots()
        {
            for (var i = 0; i < 2; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.CopPed, Position, 0f,
                    (position, heading) => EntityUtils.CreateLocalCop(Position)));
            }
        }

        private void InitializeBarriers()
        {
            var rowPosition = Position + MathHelper.ConvertHeadingToDirection(Heading - 180) * 5f;
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(Heading + 90) * 2.5f;

            for (var i = 0; i < 3; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.Barrier, startPosition, Heading + 90,
                    (position, heading) => PropUtils.CreatePoliceDoNotCrossBarrier(position)));
                startPosition += MathHelper.ConvertHeadingToDirection(Heading - 90) * 2f;
            }
        }
    }
}