using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class Inventory
    {
        public Inventory()
        {
        }

        public Inventory(string name, string scriptName, List<InventoryWeapon> weapons, StunWeapon stunWeapon)
        {
            Name = name;
            ScriptName = scriptName;
            Weapons = weapons;
            StunWeapon = stunWeapon;
        }

        public string Name { get; internal set; }

        public string ScriptName { get; internal set; }

        [XmlElement(ElementName = "Weapon")] public List<InventoryWeapon> Weapons { get; internal set; }

        [XmlElement(IsNullable = true)] public StunWeapon StunWeapon { get; internal set; }

        protected bool Equals(Inventory other)
        {
            return Name == other.Name && ScriptName == other.ScriptName && Weapons?.All(other.Weapons.Contains) == true && Equals(StunWeapon, other.StunWeapon);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Inventory)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ScriptName != null ? ScriptName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Weapons != null ? Weapons.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (StunWeapon != null ? StunWeapon.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}