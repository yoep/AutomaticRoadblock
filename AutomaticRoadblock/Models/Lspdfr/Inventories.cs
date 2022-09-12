using System.Collections.Generic;
using System.Xml.Serialization;
using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    [XmlRoot(ElementName = "Inventories")]
    public class Inventories
    {
        [XmlUnwrapContents]
        public List<Inventory> Items { get; internal set; }
    }
}