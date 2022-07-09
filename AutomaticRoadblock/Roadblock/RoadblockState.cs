namespace AutomaticRoadblocks.Roadblock
{
    public enum RoadblockState
    {
        /// <summary>
        /// Indicates that the roadblock is being prepared.
        /// </summary>
        Preparing,
        /// <summary>
        /// Indicates that the roadblock is active and spawned in the wold.
        /// </summary>
        Active,
        /// <summary>
        /// Indicates that the roadblock has been bypassed.
        /// </summary>
        Bypassed,
        /// <summary>
        /// Indicates that the suspect has hit the roadblock.
        /// </summary>
        Hit,
        /// <summary>
        /// Indicates that an error occurred when processing the roadblock.
        /// </summary>
        Error
    }
}