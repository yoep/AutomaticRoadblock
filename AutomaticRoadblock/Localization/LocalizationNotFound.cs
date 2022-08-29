using System;

namespace AutomaticRoadblocks.Localization
{
    public class LocalizationNotFound : Exception
    {
        public LocalizationNotFound(LocalizationKey key)
            : base("Localization message could not be found for " + key.Identifier)
        {
        }
    }
}