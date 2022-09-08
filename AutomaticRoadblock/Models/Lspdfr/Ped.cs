using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class Ped
    {
        [XmlUnwrapContents] 
        public string Name { get; internal set; }

        [Xml] 
        public int Chance { get; internal set; }
        
        [Xml] 
        public string Outfit { get; internal set; }
        
        [Xml] 
        public string Inventory { get; internal set; }
    }
}