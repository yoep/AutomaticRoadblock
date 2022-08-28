namespace AutomaticRoadblocks.Instance
{
    public enum InstanceState
    {
        /// <summary>
        /// Indicates that the instance is being prepared and is not yet spawned.
        /// </summary>
        Inactive,
        /// <summary>
        /// Indicates that the instance is spawning.
        /// </summary>
        Spawning,
        /// <summary>
        /// Indicates that the instance is spawned.
        /// </summary>
        Spawned,
        /// <summary>
        /// Indicates that an error occurred while spawning the instance.
        /// </summary>
        Error
    }
}