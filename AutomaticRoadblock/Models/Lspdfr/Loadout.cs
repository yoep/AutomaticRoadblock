using System.Collections.Generic;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class Loadout
    {
        public string Name { get; internal set; }

        [XmlElement(IsNullable = true)]
        [CanBeNull]
        public List<Vehicle> Vehicles { get; internal set; }

        public List<Ped> Peds { get; internal set; }

        [XmlElement(IsNullable = true)]
        [CanBeNull]
        public NumPeds NumPeds { get; internal set; }
    }
}