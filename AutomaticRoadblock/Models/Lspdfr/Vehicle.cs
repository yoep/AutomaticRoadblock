using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class Vehicle
    {
        public Vehicle()
        {
        }

        public Vehicle(string name)
        {
            Name = name;
        }

        [XmlUnwrapContents]
        public string Name { get; internal set; }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}";
        }

        protected bool Equals(Vehicle other)
        {
            return Name == other.Name;
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
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}