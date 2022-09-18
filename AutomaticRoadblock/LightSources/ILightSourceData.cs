using AutomaticRoadblocks.Data;

namespace AutomaticRoadblocks.LightSources
{
    public interface ILightSourceData : IDataFile
    {
        /// <summary>
        /// The lights model data.
        /// </summary>
        Lights Lights { get; }
    }
}