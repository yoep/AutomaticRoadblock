using System.Xml.Serialization;
using AutomaticRoadblocks.Xml.Attributes;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class Ped
    {
        public Ped()
        {
        }

        public Ped(string name, int chance, [CanBeNull] string outfit, [CanBeNull] string inventory)
        {
            Name = name;
            Chance = chance;
            Outfit = outfit;
            Inventory = inventory;
        }

        [XmlUnwrapContents] public string Name { get; internal set; }

        [XmlAttribute] public int Chance { get; internal set; } = 100;

        [XmlAttribute] [CanBeNull] public string Outfit { get; internal set; }

        [XmlAttribute] [CanBeNull] public string Inventory { get; internal set; }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Chance)}: {Chance}, {nameof(Outfit)}: {Outfit}, {nameof(Inventory)}: {Inventory}";
        }

        protected bool Equals(Ped other)
        {
            return Name == other.Name && Chance == other.Chance && Outfit == other.Outfit && Inventory == other.Inventory;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Ped)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Chance;
                hashCode = (hashCode * 397) ^ (Outfit != null ? Outfit.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Inventory != null ? Inventory.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}