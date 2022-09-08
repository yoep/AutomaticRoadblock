using AutomaticRoadblocks.Xml.Attributes;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class Agency
    {
        public string Name { get; internal set; }
        
        public string ShortName { get; internal set; }
        
        public string ScriptName { get; internal set; }
        
        [XmlElement(IsOptional = true)]
        [CanBeNull]
        public string Inventory { get; internal set; }
        
        [XmlElement(IsOptional = true)]
        [CanBeNull]
        public string TextureDictionary { get; internal set; }
        
        [XmlElement(IsOptional = true)]
        [CanBeNull]
        public string TextureName { get; internal set; }
        
        public Loadout Loadout { get; internal set; }
    }
}