using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace AutomaticRoadblocks.Lspdfr
{
    public class Loadout
    {
        public static Loadout Defaults = new(
            "Default unit",
            new List<VehicleData> { new("police"), new("police2") },
            new List<PedData> { new("s_m_y_cop_01", 70), new("s_f_y_cop_01", 30) },
            new NumPeds(1, 2));

        public Loadout()
        {
        }

        public Loadout(string name, List<VehicleData> vehicles, List<PedData> peds, NumPeds numPeds)
        {
            Name = name;
            Vehicles = vehicles;
            Peds = peds;
            NumPeds = numPeds;
        }

        public string Name { get; internal set; }

        public List<VehicleData> Vehicles { get; internal set; } = new();

        public List<PedData> Peds { get; internal set; } = new();

        [XmlElement(IsNullable = true)] public NumPeds NumPeds { get; internal set; } = new();

        public override string ToString()
        {
            return
                $"{nameof(Name)}: {Name}, {nameof(Vehicles)}: {string.Join(", ", Vehicles)},  {nameof(Peds)}: {string.Join(", ", Peds)}, " +
                $"{nameof(NumPeds)}: {NumPeds}";
        }

        protected bool Equals(Loadout other)
        {
            return Name == other.Name && Vehicles.All(x => other.Vehicles.Contains(x)) && Peds.All(x => other.Peds.Contains(x))
                   && Equals(NumPeds, other.NumPeds);
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