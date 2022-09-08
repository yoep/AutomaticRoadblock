using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class NumPeds
    {
        [Xml]
        public int Min { get; internal set; }
        
        [Xml]
        public int Max { get; internal set; }
    }
}