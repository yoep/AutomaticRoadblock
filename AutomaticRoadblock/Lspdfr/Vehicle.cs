using System.Xml.Serialization;
using AutomaticRoadblocks.Xml.Attributes;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Lspdfr
{
    public class Vehicle
    {
        public Vehicle()
        {
        }

        public Vehicle(string modelName)
        {
            ModelName = modelName;
        }

        public Vehicle(string modelName, [CanBeNull] string livery)
        {
            ModelName = modelName;
            Livery = livery;
        }

        [XmlUnwrapContents]
        public string ModelName { get; internal set; }
        
        [XmlAttribute]
        [CanBeNull]
        public string Livery { get; internal set; }

        public override string ToString()
        {
            return $"{nameof(ModelName)}: {ModelName}, {nameof(Livery)}: {Livery}";
        }

        protected bool Equals(Vehicle other)
        {
            return ModelName == other.ModelName && Livery == other.Livery;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Vehicle)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ModelName != null ? ModelName.GetHashCode() : 0) * 397) ^ (Livery != null ? Livery.GetHashCode() : 0);
            }
        }
    }
}