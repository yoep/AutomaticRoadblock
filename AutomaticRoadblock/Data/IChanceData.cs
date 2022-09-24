namespace AutomaticRoadblocks.Data
{
    public interface IChanceData
    {
        /// <summary>
        /// Get the chance value between 0 and 100.
        /// Where 100 = always and 0 = never.
        /// </summary>
        int Chance { get; }

        /// <summary>
        /// Verify if incomplete/nullable chances are allowed.
        /// A chance can return null when the total chance doesn't match 100.
        /// </summary>
        bool IsNullable { get; }
    }
}