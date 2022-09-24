using System.Collections.Generic;
using System.Linq;
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

        public WeaponData(string assetName, int chance = 100, List<string> components = null)
        {
            AssetName = assetName;
            Chance = chance;
            Component = components ?? new List<string>();
        }

        [XmlUnwrapContents(OptionalElementName = "Model")]
        public string AssetName { get; internal set; }

        /// <inheritdoc />
        [XmlAttribute]
        public int Chance { get; internal set; } = 100;

        [XmlElement(IsNullable = true)] public List<string> Component { get; internal set; } = new();

        /// <inheritdoc />
        [XmlIgnore]
        public bool IsNullable => true;

        public override string ToString()
        {
            return $"{nameof(AssetName)}: {AssetName}, {nameof(Chance)}: {Chance}, {nameof(Component)}: [{string.Join(", ", Component)}]";
        }

        protected bool Equals(WeaponData other)
        {
            return AssetName == other.AssetName && Chance == other.Chance && Component.All(x => other.Component.Contains(x));
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
                var hashCode = (AssetName != null ? AssetName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Chance;
                hashCode = (hashCode * 397) ^ (Component != null ? Component.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}