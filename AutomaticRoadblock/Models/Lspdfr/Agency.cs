using System.Xml.Serialization;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class Agency
    {
        public string Name { get; internal set; }

        public string ShortName { get; internal set; }

        public string ScriptName { get; internal set; }

        [XmlElement(IsNullable = true)]
        [CanBeNull]
        public string Inventory { get; internal set; }

        [XmlElement(IsNullable = true)]
        [CanBeNull]
        public string TextureDictionary { get; internal set; }

        [XmlElement(IsNullable = true)]
        [CanBeNull]
        public string TextureName { get; internal set; }

        public Loadout Loadout { get; internal set; }

        public override string ToString()
        {
            return
                $"{nameof(Name)}: {Name}, {nameof(ShortName)}: {ShortName}, {nameof(ScriptName)}: {ScriptName}, {nameof(Inventory)}: {Inventory}, " +
                $"{nameof(TextureDictionary)}: {TextureDictionary}, {nameof(TextureName)}: {TextureName}, {nameof(Loadout)}: {Loadout}";
        }
    }
}