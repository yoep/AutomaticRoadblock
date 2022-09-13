namespace AutomaticRoadblocks.Models
{
    /// <summary>
    /// The model data contain data of defined data files from LSPDFR & Automatic roadblocks.
    /// </summary>
    public interface IModelData
    {
        /// <summary>
        /// Reload the model data information.
        /// </summary>
        void Reload();
    }
}