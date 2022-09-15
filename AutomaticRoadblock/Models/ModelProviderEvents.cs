using System.Collections.Generic;
using AutomaticRoadblocks.Barriers;

namespace AutomaticRoadblocks.Models
{
    public static class ModelProviderEvents
    {
        public delegate void BarrierModelsChanged(IEnumerable<BarrierModel> models);
    }
}