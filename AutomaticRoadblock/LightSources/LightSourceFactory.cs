using System;
using System.Collections.Generic;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Instances;
using AutomaticRoadblocks.Roadblock;
using AutomaticRoadblocks.Roadblock.Slot;
using AutomaticRoadblocks.Utils;
using Rage;
using Object = Rage.Object;

namespace AutomaticRoadblocks.LightSources
{
    public static class LightSourceFactory
    {
        private const float PlacementDistanceRoadSides = 5f;

        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        private static readonly Random Random = new();

        public static IEnumerable<ARScenery> Create(LightModel lightModel, IRoadblock roadblock)
        {
            Assert.NotNull(lightModel, "lightModel cannot be null");
            Assert.NotNull(roadblock, "roadblock cannot be null");
            var instances = new List<ARScenery>();
            var flags = lightModel.Light.Flags;

            if (flags.HasFlag(ELightSourceFlags.RoadLeft))
            {
                instances.Add(CreateSideInstance(lightModel, roadblock.Road.LeftSide, roadblock.Road.Position, roadblock.Road.Heading, roadblock.Heading));
            }

            if (flags.HasFlag(ELightSourceFlags.RoadRight))
            {
                instances.Add(CreateSideInstance(lightModel, roadblock.Road.RightSide, roadblock.Road.Position, roadblock.Road.Heading, roadblock.Heading));
            }

            if (flags.HasFlag(ELightSourceFlags.Lane))
            {
                instances.AddRange(CreateLineOfInstances(lightModel, roadblock.Road.LeftSide, roadblock.Road.RightSide, roadblock.Heading));
            }

            return instances;
        }

        public static IEnumerable<ARScenery> Create(LightModel lightModel, IRoadblockSlot roadblockSlot)
        {
            Assert.NotNull(lightModel, "lightModel cannot be null");
            Assert.NotNull(roadblockSlot, "roadblockSlot cannot be null");
            var instances = new List<ARScenery>();
            var flags = lightModel.Light.Flags;

            if (flags.HasFlag(ELightSourceFlags.RoadLeft))
            {
                instances.Add(CreateSideInstance(lightModel, roadblockSlot.Lane.LeftSide, roadblockSlot.Lane.Position, roadblockSlot.Lane.Heading,
                    roadblockSlot.Heading));
            }

            if (flags.HasFlag(ELightSourceFlags.RoadRight))
            {
                instances.Add(CreateSideInstance(lightModel, roadblockSlot.Lane.RightSide, roadblockSlot.Lane.Position, roadblockSlot.Lane.Heading,
                    roadblockSlot.Heading));
            }

            if (flags.HasFlag(ELightSourceFlags.Lane))
            {
                instances.AddRange(CreateLineOfInstances(lightModel, roadblockSlot.Lane.LeftSide, roadblockSlot.Lane.RightSide, roadblockSlot.Heading));
            }

            return instances;
        }

        private static ARScenery CreateSideInstance(LightModel lightModel, Vector3 sidePosition, Vector3 targetPoint, float travelHeading,
            float placementHeading)
        {
            Logger.Trace($"Creating side instance at {sidePosition} for {lightModel}");
            var position = sidePosition + MathHelper.ConvertHeadingToDirection(travelHeading) * PlacementDistanceRoadSides;
            var targetPosition = targetPoint + MathHelper.ConvertHeadingToDirection(placementHeading - 180) * PlacementDistanceRoadSides;
            var heading = MathHelper.ConvertDirectionToHeading(targetPosition - position) + lightModel.Rotation;

            return new ARScenery(CreateInstance(lightModel, position, heading));
        }

        private static IEnumerable<ARScenery> CreateLineOfInstances(LightModel lightModel, Vector3 leftPosition, Vector3 rightPosition, float placementHeading)
        {
            Logger.Trace($"Creating light source instances from {leftPosition} to {rightPosition} for {lightModel}");
            var startPosition = CalculateRowPosition(leftPosition, placementHeading);
            var direction = MathHelper.ConvertHeadingToDirection(MathHelper.ConvertDirectionToHeading(rightPosition - leftPosition));
            var totalWidth = lightModel.Width + lightModel.Spacing;
            var totalLights = (int)(leftPosition.DistanceTo(rightPosition) / totalWidth);
            var instances = new List<ARScenery>();

            Logger.Trace($"Creating a total of {totalLights} light source instances");
            for (var i = 0; i < totalLights; i++)
            {
                instances.Add(new ARScenery(CreateInstance(lightModel, startPosition, Random.Next(360))));
                startPosition += direction * totalWidth;
            }

            return instances;
        }

        private static Entity CreateInstance(LightModel lightModel, Vector3 position, float heading)
        {
            return lightModel.Model != null
                ? CreateInstance(lightModel.Model.Value, position, heading)
                : CreateInstance(lightModel.WeaponAsset.Value, position, heading);
        }

        private static Entity CreateInstance(Model model, Vector3 position, float heading)
        {
            var instance = new Object(model, position, heading);
            return PropUtils.PlaceCorrectlyOnGround(instance);
        }

        private static Entity CreateInstance(WeaponAsset asset, Vector3 position, float heading)
        {
            var placementPosition = GameUtils.GetOnTheGroundPosition(position) + Vector3.WorldUp * 0.05f;
            var instance = new Weapon(asset, placementPosition, -1);

            if (instance.IsValid())
                instance.Rotation = new Rotator(heading, 90f, 0f);

            return instance;
        }

        private static Vector3 CalculateRowPosition(Vector3 position, float placementHeading)
        {
            return position + MathHelper.ConvertHeadingToDirection(placementHeading - 180) * 2f;
        }
    }
}