using System.Collections.Generic;
using System.Xml.Serialization;
using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Lspdfr
{
    [XmlRoot("Agencies")]
    public class Agencies
    {
        [XmlUnwrapContents]
        public List<Agency> Items { get; internal set; }
    }
}