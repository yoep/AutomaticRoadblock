using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Lspdfr
{
    public class Variation
    {
        public Variation()
        {
        }

        public Variation([CanBeNull] string @base, [CanBeNull] string name, [CanBeNull] string scriptName, [CanBeNull] string gender, List<OutfitComponent> components)
        {
            Base = @base;
            Name = name;
            ScriptName = scriptName;
            Gender = gender;
            Components = components;
        }

        [XmlElement(IsNullable = true)]
        [CanBeNull]
        public string Base { get; internal set; }
        
        [XmlElement(IsNullable = true)]
        [CanBeNull]
        public string Name { get; internal set; }

        [XmlElement(IsNullable = true)]
        [CanBeNull]
        public string ScriptName { get; internal set; }

        [XmlElement(IsNullable = true)]
        [CanBeNull]
        public string Gender { get; internal set; }

        public List<OutfitComponent> Components { get; internal set; }

        public override string ToString()
        {
            return
                $"{nameof(Base)}: {Base}, {nameof(Name)}: {Name}, {nameof(ScriptName)}: {ScriptName}, {nameof(Gender)}: {Gender}, " +
                $"{nameof(Components)}: [{string.Join(", ", Components)}]";
        }

        protected bool Equals(Variation other)
        {
            return Equals(Base, other.Base) && Equals(Name, other.Name) && ScriptName == other.ScriptName && Gender == other.Gender && Components.All(x => other.Components.Contains(x));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Variation)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ScriptName != null ? ScriptName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Gender != null ? Gender.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Components != null ? Components.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}