using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class Loadout
    {
        public Loadout()
        {
        }

        public Loadout(string name, [CanBeNull] List<Vehicle> vehicles, List<Ped> peds, [CanBeNull] NumPeds numPeds)
        {
            Name = name;
            Vehicles = vehicles;
            Peds = peds;
            NumPeds = numPeds;
        }

        public string Name { get; internal set; }

        [XmlElement(IsNullable = true)]
        [CanBeNull]
        public List<Vehicle> Vehicles { get; internal set; }

        public List<Ped> Peds { get; internal set; }

        [XmlElement(IsNullable = true)]
        [CanBeNull]
        public NumPeds NumPeds { get; internal set; }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Vehicles)}: [{string.Join("; ", Vehicles)}], {nameof(Peds)}: [{string.Join("; ", Peds)}], {nameof(NumPeds)}: {NumPeds}";
        }

        protected bool Equals(Loadout other)
        {
            return Name == other.Name && Vehicles?.All(other.Vehicles.Contains) == true && Peds?.All(other.Peds.Contains) == true &&
                   Equals(NumPeds, other.NumPeds);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Loadout)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Vehicles != null ? Vehicles.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Peds != null ? Peds.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NumPeds != null ? NumPeds.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}