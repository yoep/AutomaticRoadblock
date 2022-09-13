using System.Collections.Generic;
using System.Xml.Serialization;
using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Barriers
{
    [XmlRoot("Barriers")]
    public class Barriers
    {
        public static readonly Barriers Defaults = new()
        {
            Items = new List<Barrier>
            {
                new()
                {
                    Name = "Small cone",
                    ScriptName = "small_cone",
                    Model = "prop_mp_cone_03",
                    Spacing = 0.4
                },
                new()
                {
                    Name = "Small cone with stripes",
                    ScriptName = "small_cone_stripes",
                    Model = "prop_mp_cone_02",
                    Spacing = 0.4
                }
            }
        };

        public Barriers()
        {
        }

        public Barriers(List<Barrier> items)
        {
            Items = items;
        }

        [XmlUnwrapContents] public List<Barrier> Items { get; internal set; }
    }
}