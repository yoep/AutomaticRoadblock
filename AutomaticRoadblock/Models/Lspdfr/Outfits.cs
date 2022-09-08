using System.Collections.Generic;
using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    [XmlRootName("Outfits")]
    public class Outfits
    {
        [XmlUnwrapContents] 
        public List<Outfit> Items { get; internal set; }
    }
}