using System.Collections.Generic;
using System.Xml.Serialization;
using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.LightSources
{
    [XmlRoot("Lights")]
    public class Lights
    {
        public static readonly Lights Defaults = new()
        {
            Items = new List<Light>
            {
                new()
                {
                    Name = "Flares",
                    ScriptName = Light.FlaresScriptName,
                    Model = "weapon_flare",
                    Spacing = 1.0,
                    Flags = ELightSourceFlags.Lane
                },
                new()
                {
                    Name = "Spots",
                    ScriptName = Light.SpotsScriptName,
                    Model = "prop_generator_03b",
                    Flags = ELightSourceFlags.RoadLeft | ELightSourceFlags.RoadRight
                },
                new()
                {
                    Name = "Warning",
                    ScriptName = "warning",
                    Model = "prop_warninglight_01",
                    Spacing = 1.0,
                    Flags = ELightSourceFlags.Lane
                }
            }
        };

        [XmlUnwrapContents] public List<Light> Items { get; internal set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Items)}: [{string.Join(", ", Items)}]";
        }
    }
}