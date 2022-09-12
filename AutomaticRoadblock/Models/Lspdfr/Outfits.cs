using System.Collections.Generic;
using System.Xml.Serialization;
using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    [XmlRoot("Outfits")]
    public class Outfits
    {
        [XmlUnwrapContents] 
        public List<Outfit> Items { get; internal set; }
    }
}