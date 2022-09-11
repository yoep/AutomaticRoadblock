namespace AutomaticRoadblocks.Roadblock
{
    public enum ERoadblockDistance
    {
        /// <summary>
        /// The current location of the target vehicle.
        /// </summary>
        CurrentLocation,
        /// <summary>
        /// Nearby the target vehicle.
        /// </summary>
        Closely,
        /// <summary>
        /// Default based on the target vehicle speed.
        /// </summary>
        Default,
        /// <summary>
        /// Far ahead of the target vehicle.
        /// </summary>
        Far,
        VeryFar,
        ExtremelyFar
    }
}