using System.Collections.Generic;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.LightSources;

namespace AutomaticRoadblocks.Models
{
    public static class ModelProviderEvents
    {
        public delegate void BarrierModelsChanged(IEnumerable<BarrierModel> models);

        public delegate void LightModelsChanged(IEnumerable<LightModel> models);
    }
}