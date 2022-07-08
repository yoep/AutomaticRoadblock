using AutomaticRoadblocks.Menu;

namespace AutomaticRoadblocks.Debug
{
    public interface IRoadPreview : IMenuComponent
    {
        /// <summary>
        /// Show the road preview.
        /// </summary>
        void ShowRoadPreview();
        
        /// <summary>
        /// Hide the road preview.
        /// </summary>
        void HideRoadPreview();
    }
}