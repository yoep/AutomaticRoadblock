namespace AutomaticRoadblocks.Roadblock.Slot
{
    public interface IPursuitRoadblockSlot : IRoadblockSlot
    {
        /// <summary>
        /// Verify if a cop from this slot has been killed by the target vehicle.
        /// </summary>
        bool HasCopBeenKilledByTarget { get; }
    }
}