using AutomaticRoadblocks.Models.Lspdfr;

namespace AutomaticRoadblocks.Models
{
    public interface IModelProvider
    {
        /// <summary>
        /// Load the model provider information.
        /// </summary>
        void Load();
    }
}