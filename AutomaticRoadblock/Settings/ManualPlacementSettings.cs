namespace AutomaticRoadblocks.Settings
{
    public class ManualPlacementSettings
    {
        /// <summary>
        /// Enable a preview of the roadblock that will be placed (a marker will be shown otherwise)
        /// It's only recommended to turn on this feature on high spec computers
        /// </summary>
        public bool EnablePreview { get; internal set; }
    }
}