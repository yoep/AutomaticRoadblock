using AutomaticRoadblocks.Preview;
using Rage;

namespace AutomaticRoadblocks.Scenery
{
    public interface ISceneryItem : IPreviewSupport
    {
        /// <summary>
        /// Get the position of the scenery item.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Get the heading of the scenery item.
        /// </summary>
        float Heading { get; }
    }
}