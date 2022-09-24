using System.Xml.Serialization;

namespace AutomaticRoadblocks.Lspdfr
{
    public class NumPeds
    {
        public NumPeds()
        {
        }

        public NumPeds(int min, int max)
        {
            Min = min;
            Max = max;
        }

        [XmlAttribute] public int Min { get; internal set; } = 1;

        [XmlAttribute] public int Max { get; internal set; } = 1;

        public override string ToString()
        {
            return $"{nameof(Min)}: {Min}, {nameof(Max)}: {Max}";
        }

        protected bool Equals(NumPeds other)
        {
            return Min == other.Min && Max == other.Max;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NumPeds)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Min * 397) ^ Max;
            }
        }
    }
}