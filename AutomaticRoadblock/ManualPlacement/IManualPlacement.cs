using AutomaticRoadblocks.Utils.Road;

namespace AutomaticRoadblocks.ManualPlacement
{
    public interface IManualPlacement
    {
        /// <summary>
        /// Determine the placement location based on the current player location.
        /// </summary>
        /// <returns>Returns the road for the placement.</returns>
        Road DetermineLocation();

        /// <summary>
        /// Create a preview of the roadblock that will be placed. 
        /// </summary>
        void CreatePreview();

        /// <summary>
        /// Remove the preview of the roadblock that will be placed.
        /// </summary>
        void RemovePreview();

        /// <summary>
        /// Place a roadblock based on the <see cref="DetermineLocation"/> <see cref="Road"/>.
        /// </summary>
        void PlaceRoadblock();
    }
}