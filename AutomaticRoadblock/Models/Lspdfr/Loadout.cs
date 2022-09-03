using System.Collections.Generic;
using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class Loadout
    {
        public string Name { get; internal set; }

        [XmlElement(IsOptional = true)]
        public List<Vehicle> Vehicles { get; internal set; }
    }
}