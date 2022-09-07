namespace AutomaticRoadblocks.Street
{
    public enum ETraceFlags : uint
    {
        None = 0,
        IntersectWorld = 1,
        IntersectVehicles = 2,
        IntersectPedsSimpleCollision = 4,
        IntersectPeds = 8,
        IntersectObjects = 16,
        IntersectWater = 32,
        Unknown = 128,
        IntersectFoliage = 256,
        IntersectEverything = 4294967295
    }
}