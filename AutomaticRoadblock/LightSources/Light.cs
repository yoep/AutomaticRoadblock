using System.Xml.Serialization;

namespace AutomaticRoadblocks.LightSources
{
    public class Light
    {
        public const string FlaresScriptName = "flares";
        public const string SpotsScriptName = "spots";
        public const string GroundSpotsScriptName = "ground_spots";

        public Light()
        {
        }

        public Light(string name, string scriptName, string model, ELightSourceFlags flags)
        {
            Name = name;
            ScriptName = scriptName;
            Model = model;
            Flags = flags;
        }

        public Light(string name, string scriptName, string model, double spacing, ELightSourceFlags flags)
        {
            Name = name;
            ScriptName = scriptName;
            Model = model;
            Spacing = spacing;
            Flags = flags;
        }

        public Light(string name, string scriptName, string model, double spacing, double rotation, ELightSourceFlags flags)
        {
            Name = name;
            ScriptName = scriptName;
            Model = model;
            Spacing = spacing;
            Rotation = rotation;
            Flags = flags;
        }

        public string Name { get; internal set; }
        
        public string ScriptName { get; internal set; }
        
        public string Model { get; internal set; }

        /// <summary>
        /// The spacing between each light.
        /// </summary>
        [XmlElement(IsNullable = true)] public double Spacing { get; internal set; }
        
        /// <summary>
        /// The relative rotation when placing the light.
        /// </summary>
        [XmlElement(IsNullable = true)] public double Rotation { get; internal set; }

        [XmlElement(IsNullable = true)] public ELightSourceFlags Flags { get; internal set; } = ELightSourceFlags.None;

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(ScriptName)}: {ScriptName}, {nameof(Model)}: {Model}, " +
                   $"{nameof(Spacing)}: {Spacing}, {nameof(Rotation)}: {Rotation}, {nameof(Flags)}: {Flags}";
        }

        protected bool Equals(Light other)
        {
            return Name == other.Name && ScriptName == other.ScriptName && Model == other.Model && Spacing.Equals(other.Spacing) && Rotation.Equals(other.Rotation) && Flags == other.Flags;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Light)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ScriptName != null ? ScriptName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Model != null ? Model.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Spacing.GetHashCode();
                hashCode = (hashCode * 397) ^ Rotation.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Flags;
                return hashCode;
            }
        }
    }
}