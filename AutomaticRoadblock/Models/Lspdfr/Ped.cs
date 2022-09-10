using AutomaticRoadblocks.Xml.Attributes;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class Ped
    {
        [XmlUnwrapContents] 
        public string Name { get; internal set; }

        [Xml(DefaultValue = 100)] 
        public int Chance { get; internal set; }
        
        [Xml(IsOptional = true)] 
        [CanBeNull]
        public string Outfit { get; internal set; }
        
        [Xml(IsOptional = true)] 
        [CanBeNull]
        public string Inventory { get; internal set; }
    }
}