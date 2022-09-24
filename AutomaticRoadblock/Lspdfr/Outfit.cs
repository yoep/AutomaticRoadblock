using System.Collections.Generic;
using System.Linq;

namespace AutomaticRoadblocks.Lspdfr
{
    public class Outfit
    {
        public Outfit()
        {
        }

        public Outfit(string name, string scriptName, List<Variation> variations)
        {
            Name = name;
            ScriptName = scriptName;
            Variations = variations;
        }

        public string Name { get; internal set; }

        public string ScriptName { get; internal set; }

        public List<Variation> Variations { get; internal set; }

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
    }
}