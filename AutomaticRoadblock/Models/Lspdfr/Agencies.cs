using System.Collections.Generic;
using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    [XmlRootName("Agencies")]
    public class Agencies
    {
        [XmlUnwrapContents]
        public List<Agency> Items { get; internal set; }
    }
}