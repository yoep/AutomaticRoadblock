using System.Xml.Serialization;
using AutomaticRoadblocks.Data;
using AutomaticRoadblocks.Xml.Attributes;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Lspdfr
{
    public class PedData : IChanceData
    {
        public PedData()
        {
        }

        public PedData(string modelName, int chance)
        {
            ModelName = modelName;
            Chance = chance;
        }

        public PedData(string modelName, int chance, string outfit, string inventory, bool helmet = false)
        {
            ModelName = modelName;
            Chance = chance;
            Outfit = outfit;
            Inventory = inventory;
            Helmet = helmet;
        }

        [XmlUnwrapContents] public string ModelName { get; internal set; }

        [XmlAttribute] public int Chance { get; internal set; } = 100;

        [XmlAttribute] [CanBeNull] public string Outfit { get; internal set; }

        [XmlAttribute] [CanBeNull] public string Inventory { get; internal set; }

        [XmlAttribute] public bool Helmet { get; internal set; }

        public override string ToString()
        {
            return $"{nameof(ModelName)}: {ModelName}, {nameof(Chance)}: {Chance}, {nameof(Outfit)}: {Outfit}, " +
                   $"{nameof(Inventory)}: {Inventory}, {nameof(Helmet)}: {Helmet}";
        }

        protected bool Equals(PedData other)
        {
            return ModelName == other.ModelName && Chance == other.Chance && Outfit == other.Outfit && Inventory == other.Inventory && Helmet == other.Helmet;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PedData)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ModelName != null ? ModelName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Chance;
                hashCode = (hashCode * 397) ^ (Outfit != null ? Outfit.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Inventory != null ? Inventory.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Helmet.GetHashCode();
                return hashCode;
            }
        }
    }
}