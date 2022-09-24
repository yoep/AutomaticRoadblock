using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace AutomaticRoadblocks.Lspdfr
{
    public class Outfit
    {
        #region Constructors

        public Outfit()
        {
        }

        public Outfit(string name, string scriptName, List<Variation> variations)
        {
            Name = name;
            ScriptName = scriptName;
            Variations = variations;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Retrieve the variation with the given script name.
        /// </summary>
        /// <param name="scriptName">The script name to retrieve.</param>
        [XmlIgnore]
        public Variation this[string scriptName] => Variations.First(x => scriptName.Equals(x.ScriptName, StringComparison.InvariantCulture));

        public string Name { get; internal set; }

        public string ScriptName { get; internal set; }

        public List<Variation> Variations { get; internal set; }

        #endregion

        #region Methods

        /// <summary>
        /// Retrieve the variations for the given gender which are not a base variation.
        /// </summary>
        /// <param name="gender">The gender to retrieve the variations of.</param>
        /// <returns>Returns the available variations.</returns>
        public List<Variation> RetrieveVariations(Gender gender)
        {
            return Variations
                .Where(x => !x.IsBase)
                .Where(x => x.Gender == gender)
                .ToList();
        }

        #endregion

        #region Formatting & Equality

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(ScriptName)}: {ScriptName}, {nameof(Variations)}: [{string.Join(", ", Variations)}]";
        }

        protected bool Equals(Outfit other)
        {
            return Name == other.Name && ScriptName == other.ScriptName && Variations.All(x => other.Variations.Contains(x));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Outfit)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ScriptName != null ? ScriptName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Variations != null ? Variations.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}