using System.Collections.Generic;
using System.Xml.Serialization;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Roadblock.Data
{
    [XmlRoot("Roadblocks")]
    public class Roadblocks
    {
        public static readonly Roadblocks Defaults = new()
        {
            Items = new List<RoadblockData>
            {
                new()
                {
                    Level = 1,
                    MainBarrier = Barrier.SmallConeStripesScriptName
                },
                new ()
                {
                    Level = 2,
                    MainBarrier = Barrier.BigConeScriptName
                },
                new ()
                {
                    Level = 3,
                    MainBarrier = Barrier.PoliceDoNotCrossScriptName
                },
                new ()
                {
                    Level = 4,
                    MainBarrier = Barrier.PoliceDoNotCrossScriptName,
                    ChaseVehicleBarrier = Barrier.BarrelScriptName
                },
                new ()
                {
                    Level = 5,
                    MainBarrier = Barrier.PoliceDoNotCrossScriptName,
                    ChaseVehicleBarrier = Barrier.BarrelScriptName
                }
            }
        };

        [XmlUnwrapContents] public List<RoadblockData> Items { get; internal set; }
    }
}