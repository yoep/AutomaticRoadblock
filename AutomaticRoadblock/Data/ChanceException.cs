using System;
using System.Collections.Generic;

namespace AutomaticRoadblocks.Data
{
    public class ChanceException<T> : Exception where T : IChanceData
    {
        public ChanceException(IEnumerable<T> items)
            : base($"Unable to retrieve chance, data is invalid for {items}")
        {
        }
    }
}