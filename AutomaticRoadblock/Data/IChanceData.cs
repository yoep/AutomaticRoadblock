namespace AutomaticRoadblocks.Data
{
    public interface IChanceData
    {
        /// <summary>
        /// Get the chance value between 0 and 100.
        /// Where 100 = always and 0 = never.
        /// </summary>
        int Chance { get; }
    }
}