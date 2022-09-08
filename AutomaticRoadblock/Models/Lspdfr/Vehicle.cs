using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class Vehicle
    {
        [XmlUnwrapContents]
        public string Name { get; internal set; }
    }
}