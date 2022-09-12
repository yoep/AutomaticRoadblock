using System.Xml.Serialization;
using AutomaticRoadblocks.Xml.Attributes;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class Ped
    {
        [XmlUnwrapContents] public string Name { get; internal set; }

        [XmlAttribute] public int Chance { get; internal set; } = 100;

        [XmlAttribute] [CanBeNull] public string Outfit { get; internal set; }

        [XmlAttribute] [CanBeNull] public string Inventory { get; internal set; }
    }
}