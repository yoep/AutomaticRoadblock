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
        /// Indicates that the cop instances of the roadblock have been released to LSPDFR.
        /// </summary>
        Released,
        /// <summary>
        /// Indicates that an error occurred when processing the roadblock.
        /// </summary>
        Error,
        /// <summary>
        /// Indicates that the roadblock is disposing and entities will be removed.
        /// </summary>
        Disposing,
        /// <summary>
        /// Indicates that the roadblock is disposed and entities have been removed.
        /// </summary>
        Disposed,
        /// <summary>
        /// Indicates that one or more crucial instances of the roadblock have been invalidated by the game engine.
        /// </summary>
        Invalid
    }
}