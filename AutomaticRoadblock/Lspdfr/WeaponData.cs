using System.Xml.Serialization;
using AutomaticRoadblocks.Data;
using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Lspdfr
{
    public class WeaponData : IChanceData
    {
        public WeaponData()
        {
        }

        public WeaponData(string assetName, int chance = 100)
        {
            AssetName = assetName;
            Chance = chance;
        }

        [XmlUnwrapContents(OptionalElementName = "Model")]
        public string AssetName { get; internal set; }

        /// <inheritdoc />
        [XmlAttribute]
        public int Chance { get; internal set; } = 100;

        /// <inheritdoc />
        [XmlIgnore]
        public bool IsNullable => true;

        public override string ToString()
        {
            return $"{nameof(AssetName)}: {AssetName}, {nameof(Chance)}: {Chance}";
        }

        protected bool Equals(WeaponData other)
        {
            return AssetName == other.AssetName && Chance == other.Chance;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WeaponData)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((AssetName != null ? AssetName.GetHashCode() : 0) * 397) ^ Chance;
            }
        }
    }
}