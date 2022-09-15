using System.Xml.Serialization;

namespace AutomaticRoadblocks.Barriers
{
    public class Barrier
    {
        public const string SmallConeScriptName = "small_cone";
        public const string SmallConeStripesScriptName = "small_cone_stripes";
        public const string BigConeScriptName = "big_cone";
        public const string BigConeStripesScriptName = "big_cone_stripes";
        public const string PoliceDoNotCrossScriptName = "police_do_not_cross";
        public const string BarrelScriptName = "barrel_traffic_catcher";

        public Barrier()
        {
        }

        public Barrier(string name, string scriptName, string model, double spacing, EBarrierFlags flags)
        {
            Name = name;
            ScriptName = scriptName;
            Model = model;
            Spacing = spacing;
            Flags = flags;
        }

        public string Name { get; internal set; }

        public string ScriptName { get; internal set; }

        public string Model { get; internal set; }

        public double Spacing { get; internal set; }

        [XmlElement(IsNullable = true)] public double VerticalOffset { get; internal set; }

        public EBarrierFlags Flags { get; internal set; }

        public override string ToString()
        {
            return
                $"{nameof(Name)}: {Name}, {nameof(ScriptName)}: {ScriptName}, {nameof(Model)}: {Model}, {nameof(Spacing)}: {Spacing}, {nameof(VerticalOffset)}: {VerticalOffset}, {nameof(Flags)}: {Flags}";
        }

        protected bool Equals(Barrier other)
        {
            return Name == other.Name && ScriptName == other.ScriptName && Model == other.Model && Spacing.Equals(other.Spacing) &&
                   VerticalOffset.Equals(other.VerticalOffset) && Flags == other.Flags;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Barrier)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ScriptName != null ? ScriptName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Model != null ? Model.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Spacing.GetHashCode();
                hashCode = (hashCode * 397) ^ VerticalOffset.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Flags;
                return hashCode;
            }
        }
    }
}