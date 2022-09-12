using System.Xml.Serialization;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class StunWeapon
    {
        public StunWeapon()
        {
        }

        public StunWeapon(string name, int chance)
        {
            Name = name;
            Chance = chance;
        }

        public string Name { get; internal set; }

        [XmlAttribute] public int Chance { get; internal set; } = 100;

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Chance)}: {Chance}";
        }

        protected bool Equals(StunWeapon other)
        {
            return Name == other.Name && Chance == other.Chance;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StunWeapon)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ Chance;
            }
        }
    }
}