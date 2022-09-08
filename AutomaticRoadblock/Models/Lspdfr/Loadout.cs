using System.Collections.Generic;
using AutomaticRoadblocks.Xml.Attributes;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class Loadout
    {
        public string Name { get; internal set; }

        [XmlElement(IsOptional = true)]
        [CanBeNull]
        public List<Vehicle> Vehicles { get; internal set; }
        
        [XmlElement]
        public List<Ped> Peds { get; internal set; }
        
        [XmlElement(IsOptional = true)]
        [CanBeNull]
        public NumPeds NumPeds { get; internal set; }
    }
}