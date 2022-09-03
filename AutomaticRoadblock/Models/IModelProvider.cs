using AutomaticRoadblocks.Models.Lspdfr;

namespace AutomaticRoadblocks.Models
{
    public interface IModelProvider
    {
        /// <summary>
        /// The agency model data.
        /// </summary>
        Agencies Agencies { get; }
    }
}