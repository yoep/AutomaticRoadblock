using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Lspdfr
{
    public class Agency
    {
        public Agency()
        {
        }

        public Agency(string name, string shortName, string scriptName, [CanBeNull] string inventory, [CanBeNull] string textureDictionary,
            [CanBeNull] string textureName, List<Loadout> loadout)
        {
            Name = name;
            ShortName = shortName;
            ScriptName = scriptName;
            Inventory = inventory;
            TextureDictionary = textureDictionary;
            TextureName = textureName;
            Loadout = loadout;
        }

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

        public List<Loadout> Loadout { get; internal set; }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(ShortName)}: {ShortName}, {nameof(ScriptName)}: {ScriptName}, {nameof(Inventory)}: {Inventory}, " +
                   $"{nameof(TextureDictionary)}: {TextureDictionary}, {nameof(TextureName)}: {TextureName}, {nameof(Loadout)}: [{string.Join(", ", Loadout)}]";
        }

        protected bool Equals(Agency other)
        {
            return Name == other.Name && ShortName == other.ShortName && ScriptName == other.ScriptName && Inventory == other.Inventory &&
                   TextureDictionary == other.TextureDictionary && TextureName == other.TextureName && Loadout.All(x => other.Loadout.Contains(x));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Agency)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ShortName != null ? ShortName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ScriptName != null ? ScriptName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Inventory != null ? Inventory.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TextureDictionary != null ? TextureDictionary.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TextureName != null ? TextureName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Loadout != null ? Loadout.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}