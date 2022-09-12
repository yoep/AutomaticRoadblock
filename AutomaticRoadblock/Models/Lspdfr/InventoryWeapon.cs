using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using AutomaticRoadblocks.Xml.Attributes;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class InventoryWeapon
    {
        public InventoryWeapon()
        {
        }

        public InventoryWeapon(string name, int chance)
        {
            Name = name;
            Components = new List<string>();
            Chance = chance;
        }

        public InventoryWeapon(string name, [CanBeNull] List<string> components, int chance)
        {
            Name = name;
            Components = components;
            Chance = chance;
        }

        [XmlUnwrapContents(OptionalElementName = "Model")]
        public string Name { get; internal set; }
        
        [XmlElement(ElementName = "Component", IsNullable = true)]
        [CanBeNull]
        public List<string> Components { get; internal set; }

        [XmlAttribute] 
        public int Chance { get; internal set; } = 100;

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Components)}: {Components}, {nameof(Chance)}: {Chance}";
        }

        protected bool Equals(InventoryWeapon other)
        {
            return Name == other.Name && Components?.All(other.Components.Contains) == true && Chance == other.Chance;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((InventoryWeapon)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Components != null ? Components.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Chance;
                return hashCode;
            }
        }
    }
}