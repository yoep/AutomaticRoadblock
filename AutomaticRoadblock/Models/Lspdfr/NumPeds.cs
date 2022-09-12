using System.Xml.Serialization;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class NumPeds
    {
        [XmlAttribute] public int Min { get; internal set; }

        [XmlAttribute] public int Max { get; internal set; }
    }
}