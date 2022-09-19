using System.Collections.Generic;
using System.Linq;

namespace AutomaticRoadblocks.Lspdfr
{
    public class Loadout
    {
        public Loadout()
        {
        }

        public Loadout(string name, List<VehicleData> vehicles)
        {
            Name = name;
            Vehicles = vehicles;
        }

        public string Name { get; internal set; }

        public List<VehicleData> Vehicles { get; internal set; } = new();

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Vehicles)}: {Vehicles}";
        }

        protected bool Equals(Loadout other)
        {
            return Name == other.Name && Vehicles.All(x => other.Vehicles.Contains(x));
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
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Vehicles != null ? Vehicles.GetHashCode() : 0);
            }
        }
    }
}