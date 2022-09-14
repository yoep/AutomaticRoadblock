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
                    ScriptName = Barrier.SmallConeScriptName,
                    Model = "prop_mp_cone_03",
                    Spacing = 0.4,
                    Flags = EBarrierFlags.All
                },
                new()
                {
                    Name = "Small cone with stripes",
                    ScriptName = Barrier.SmallConeStripesScriptName,
                    Model = "prop_mp_cone_02",
                    Spacing = 0.4,
                    Flags = EBarrierFlags.All
                },
                new()
                {
                    Name = "Big cone",
                    ScriptName = Barrier.BigConeScriptName,
                    Model = "prop_roadcone01c",
                    Spacing = 0.5,
                    Flags = EBarrierFlags.All
                },
                new()
                {
                    Name = "Big cone with stripes",
                    ScriptName = Barrier.BigConeStripesScriptName,
                    Model = "prop_mp_cone_01",
                    Spacing = 0.5,
                    Flags = EBarrierFlags.All
                },
                new()
                {
                    Name = "Police do not cross",
                    ScriptName = Barrier.PoliceDoNotCrossScriptName,
                    Model = "prop_barrier_work05",
                    Spacing = 0.2,
                    Flags = EBarrierFlags.ManualPlacement
                },
                new()
                {
                    Name = "Barrel",
                    ScriptName = Barrier.BarrelScriptName,
                    Model = "prop_barrier_wat_03b",
                    Spacing = 0.5,
                    Flags = EBarrierFlags.ManualPlacement
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