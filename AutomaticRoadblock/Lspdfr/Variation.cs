using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Lspdfr
{
    public class Variation
    {
        #region Constructors

        public Variation()
        {
        }

        public Variation([CanBeNull] string @base, [CanBeNull] string name, [CanBeNull] string scriptName, Gender gender, List<OutfitComponent> components)
        {
            Base = @base;
            Name = name;
            ScriptName = scriptName;
            Gender = gender;
            Components = components;
        }

        #endregion

        #region Properties

        [XmlElement(IsNullable = true)]
        [CanBeNull]
        public string Base { get; internal set; }

        [XmlElement(IsNullable = true)]
        [CanBeNull]
        public string Name { get; internal set; }

        [XmlElement(IsNullable = true)]
        [CanBeNull]
        public string ScriptName { get; internal set; }

        [XmlElement(IsNullable = true)] public Gender Gender { get; internal set; }

        public List<OutfitComponent> Components { get; internal set; }

        /// <summary>
        /// Verify if this variation is a base for other variations.
        /// </summary>
        [XmlIgnore]
        public bool IsBase => !string.IsNullOrWhiteSpace(ScriptName);

        /// <summary>
        /// Verify if this variation has a base variation defined.
        /// </summary>
        [XmlIgnore]
        public bool IsBaseDefined => !string.IsNullOrWhiteSpace(Base);

        #endregion

        #region Formatting & Equality

        public override string ToString()
        {
            return
                $"{nameof(Base)}: {Base}, {nameof(Name)}: {Name}, {nameof(ScriptName)}: {ScriptName}, {nameof(Gender)}: {Gender}, " +
                $"{nameof(Components)}: [{{{string.Join("}, {", Components)}}}]";
        }

        protected bool Equals(Variation other)
        {
            return Equals(Base, other.Base) && Equals(Name, other.Name) && ScriptName == other.ScriptName && Gender == other.Gender &&
                   Components.All(x => other.Components.Contains(x));
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
                hashCode = (hashCode * 397) ^ Gender.GetHashCode();
                hashCode = (hashCode * 397) ^ (Components != null ? Components.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}