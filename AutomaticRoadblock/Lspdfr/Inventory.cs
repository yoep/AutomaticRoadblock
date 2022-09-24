using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Lspdfr
{
    public class Inventory
    {
        public Inventory()
        {
        }

        public Inventory(string name, string scriptName, List<WeaponData> weapon, WeaponData stunWeapon = null, int armor = 0)
        {
            Name = name;
            ScriptName = scriptName;
            Weapon = weapon;
            StunWeapon = stunWeapon;
            Armor = armor;
        }

        #region Properties

        public string Name { get; internal set; }

        public string ScriptName { get; internal set; }

        public List<WeaponData> Weapon { get; internal set; }

        [XmlElement(IsNullable = true)]
        [CanBeNull]
        public WeaponData StunWeapon { get; internal set; }

        [XmlElement(IsNullable = true)] public int Armor { get; internal set; }

        /// <summary>
        /// Verify if the stun weapon is configured.
        /// </summary>
        [XmlIgnore]
        public bool IsStunWeaponAvailable => StunWeapon != null;

        #endregion

        #region Methods

        public override string ToString()
        {
            return
                $"{nameof(Name)}: {Name}, {nameof(ScriptName)}: {ScriptName}, {nameof(Weapon)}: [{string.Join(", ", Weapon)}], " +
                $"{nameof(StunWeapon)}: {StunWeapon}, {nameof(Armor)}: {Armor}";
        }

        protected bool Equals(Inventory other)
        {
            return Name == other.Name && ScriptName == other.ScriptName && Weapon.All(x => other.Weapon.Contains(x))
                   && Equals(StunWeapon, other.StunWeapon) && Equals(Armor, other.Armor);
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
                hashCode = (hashCode * 397) ^ (Weapon != null ? Weapon.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (StunWeapon != null ? StunWeapon.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Armor.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}