using System.Windows.Forms;

namespace AutomaticRoadblocks.Settings
{
    public class CloseRoadSettings
    {
        /// <summary>
        /// Close the nearby road by pressing the following key.
        /// </summary>
        public Keys CloseRoadKey { get; internal set; }
        
        /// <summary>
        /// Close the nearby road by pressing the following modifier key.
        /// </summary>
        public Keys CloseRoadModifierKey { get; internal set; }
        
        /// <summary>
        /// The maximum distance from the player the road may be closed.
        /// </summary>
        public float MaxDistanceFromPlayer { get; internal set; }
        
        /// <summary>
        /// The barrier to use when closing the road.
        /// </summary>
        public string Barrier { get; internal set; }
    }
}