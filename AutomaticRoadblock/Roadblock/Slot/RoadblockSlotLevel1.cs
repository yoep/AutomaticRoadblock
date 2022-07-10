using AutomaticRoadblocks.Instance;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.Roadblock.Slot
{
    public class RoadblockSlotLevel1 : AbstractRoadblockSlot
    {
        internal RoadblockSlotLevel1(Vector3 position, float heading) : base(position, heading)
        {
            Init();
        }

        private void Init()
        {
            InitializeVehicleSlot();
            InitializePedSlots();
            InitializeCones();
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

        private void InitializeCones()
        {
            var rowPosition = Position + MathHelper.ConvertHeadingToDirection(Heading - 180) * 5f;
            var startPosition = rowPosition + MathHelper.ConvertHeadingToDirection(Heading + 90) * 2.5f;

            for (var i = 0; i < 5; i++)
            {
                Instances.Add(new InstanceSlot(EntityType.Cone, startPosition, Heading,
                    (position, heading) => PropUtils.CreateSmallConeWithStripes(position)));
                startPosition += MathHelper.ConvertHeadingToDirection(Heading - 90) * 1.5f;
            }
        }
    }
}