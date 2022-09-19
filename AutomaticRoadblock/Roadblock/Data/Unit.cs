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

        public Unit(EBackupUnit type)
        {
            Type = type;
        }

        public Unit(EBackupUnit type, int chance)
        {
            Type = type;
            Chance = chance;
        }

        [XmlUnwrapContents] public EBackupUnit Type { get; internal set; }

        [XmlAttribute] public int Chance { get; internal set; } = 100;

        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, {nameof(Chance)}: {Chance}";
        }

        protected bool Equals(Unit other)
        {
            return Type == other.Type && Chance == other.Chance;
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
                return ((int)Type * 397) ^ Chance;
            }
        }
    }
}