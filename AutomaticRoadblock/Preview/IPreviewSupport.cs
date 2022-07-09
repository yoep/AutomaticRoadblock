namespace AutomaticRoadblocks.Preview
{
    /// <summary>
    /// Indicates that the instance can create a preview of itself in the game world.
    /// </summary>
    public interface IPreviewSupport
    {
        /// <summary>
        /// Get if a preview is being shown in the game world for the instance.
        /// </summary>
        bool IsPreviewActive { get; }
        
        /// <summary>
        /// Create a preview of the instance in the game world.
        /// </summary>
        void CreatePreview();

        /// <summary>
        /// Deletes the preview of the instance in the game world.
        /// </summary>
        void DeletePreview();
    }
}