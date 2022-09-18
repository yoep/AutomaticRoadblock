using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace AutomaticRoadblocks.Roadblock.Data
{
    public class RoadblockData
    {
        public RoadblockData()
        {
        }

        public RoadblockData(int level, string mainBarrier)
        {
            Level = level;
            MainBarrier = mainBarrier;
        }

        public RoadblockData(int level, string mainBarrier, List<string> lights, List<Unit> units)
        {
            Level = level;
            MainBarrier = mainBarrier;
            Lights = lights;
            Units = units;
        }

        public RoadblockData(int level, string mainBarrier, string secondaryBarrier, List<Unit> units)
        {
            Level = level;
            MainBarrier = mainBarrier;
            SecondaryBarrier = secondaryBarrier;
            Units = units;
        }

        public RoadblockData(int level, string mainBarrier, string secondaryBarrier, string chaseVehicleBarrier, List<string> lights, List<Unit> units)
        {
            Level = level;
            MainBarrier = mainBarrier;
            SecondaryBarrier = secondaryBarrier;
            ChaseVehicleBarrier = chaseVehicleBarrier;
            Units = units;
        }

        public int Level { get; internal set; }

        public string MainBarrier { get; internal set; }

        [XmlElement(IsNullable = true)] public string SecondaryBarrier { get; internal set; }

        [XmlElement(IsNullable = true)] public string ChaseVehicleBarrier { get; internal set; }

        [XmlElement(IsNullable = true)] public List<string> Lights { get; private set; } = new();

        public List<Unit> Units { get; internal set; }

        public override string ToString()
        {
            return $"{nameof(Level)}: {Level}, {nameof(MainBarrier)}: {MainBarrier}, {nameof(SecondaryBarrier)}: {SecondaryBarrier}, " +
                   $"{nameof(ChaseVehicleBarrier)}: {ChaseVehicleBarrier}, {nameof(Lights)}: [{string.Join(", ", Lights)}], " +
                   $"{nameof(Units)}: [{string.Join(", ", Units)}]";
        }

        protected bool Equals(RoadblockData other)
        {
            return Level == other.Level && MainBarrier == other.MainBarrier && SecondaryBarrier == other.SecondaryBarrier &&
                   ChaseVehicleBarrier == other.ChaseVehicleBarrier && Lights.All(x => other.Lights.Contains(x)) && Units.All(x => other.Units.Contains(x));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RoadblockData)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Level;
                hashCode = (hashCode * 397) ^ (MainBarrier != null ? MainBarrier.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SecondaryBarrier != null ? SecondaryBarrier.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ChaseVehicleBarrier != null ? ChaseVehicleBarrier.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Lights != null ? Lights.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}