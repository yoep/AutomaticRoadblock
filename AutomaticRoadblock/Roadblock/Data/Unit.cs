using System.Xml.Serialization;
using AutomaticRoadblocks.Data;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Roadblock.Data
{
    public class Unit : IChanceData
    {
        public Unit()
        {
        }

        public Unit(EBackupUnit name)
        {
            Name = name;
        }

        public Unit(EBackupUnit name, int chance)
        {
            Name = name;
            Chance = chance;
        }

        [XmlUnwrapContents] public EBackupUnit Name { get; internal set; }

        [XmlAttribute] public int Chance { get; internal set; } = 100;

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Chance)}: {Chance}";
        }

        protected bool Equals(Unit other)
        {
            return Name == other.Name && Chance == other.Chance;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Unit)obj);
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